using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Staff;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE.Services;

public sealed class StaffService : IStaffService
{
    private readonly RestaurantDB _db;
    public StaffService(RestaurantDB db) => _db = db;

    public async Task<StaffOverviewDTO> GetOverviewAsync(
        Guid restaurantId, string actorUserId, CancellationToken ct)
    {
        var org = await RequireOwnerAsync(restaurantId, actorUserId, ct);
        var plan = await GetPlanAsync(org, ct);
        var used = await UsedEmployeesAsync(org.Id, org.OwnerUserId, ct);
        var members = await _db.RestaurantUsers.AsNoTracking()
            .Where(x => x.RestaurantId == restaurantId)
            .OrderBy(x => x.User.Email)
            .Select(x => new StaffMemberDTO
            {
                UserId = x.UserId,
                Email = x.User.Email ?? "",
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Role = x.Role,
                JoinedAt = x.JoinedAt
            }).ToListAsync(ct);
        return new StaffOverviewDTO
        {
            RestaurantId = restaurantId,
            OrganizationId = org.Id,
            MaxEmployees = plan.MaxEmployees,
            UsedEmployees = used,
            Members = members
        };
    }

    public async Task<List<StaffInvitationDTO>> GetPendingInvitationsAsync(
        Guid restaurantId, string actorUserId, CancellationToken ct)
    {
        await RequireOwnerAsync(restaurantId, actorUserId, ct);
        var now = DateTimeOffset.UtcNow;
        return await _db.StaffInvitations.AsNoTracking()
            .Where(i => i.RestaurantId == restaurantId &&
                        i.AcceptedAt == null && i.RevokedAt == null &&
                        i.ExpiresAt > now)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new StaffInvitationDTO
            {
                Id = i.Id, Email = i.Email, Role = i.Role, ExpiresAt = i.ExpiresAt
            }).ToListAsync(ct);
    }

    public async Task<CreatedStaffInvitationDTO> InviteAsync(
        Guid restaurantId, string actorUserId, InviteStaffDTO dto, CancellationToken ct)
    {
        ValidateStaffRole(dto.Role);
        var email = (dto.Email ?? "").Trim();
        if (email.Length > 256 || !new EmailAddressAttribute().IsValid(email))
            throw new ArgumentException("Enter a valid email address.");
        var normalizedEmail = NormalizeEmail(email);
        var org = await RequireOwnerAsync(restaurantId, actorUserId, ct);
        await EnsureEntitledAsync(org, ct);
        var plan = await GetPlanAsync(org, ct);
        var used = await UsedEmployeesAsync(org.Id, org.OwnerUserId, ct);

        // If user already works for this organization, a second branch does not
        // consume another seat. Unknown accounts require a free seat.
        var existingUserId = await _db.Users.AsNoTracking()
            .Where(u => u.NormalizedEmail == normalizedEmail)
            .Select(u => u.Id).SingleOrDefaultAsync(ct);
        var alreadyInOrganization = existingUserId != null &&
            await _db.RestaurantUsers.AsNoTracking()
                .AnyAsync(x => x.UserId == existingUserId &&
                    x.Restaurant.OrganizationId == org.Id, ct);
        if (used >= plan.MaxEmployees && !alreadyInOrganization)
            throw new InvalidOperationException("Employee limit reached. Upgrade your plan.");

        if (existingUserId != null && await _db.RestaurantUsers.AsNoTracking()
                .AnyAsync(x => x.RestaurantId == restaurantId &&
                               x.UserId == existingUserId, ct))
            throw new InvalidOperationException("This user is already in this restaurant.");

        var now = DateTimeOffset.UtcNow;
        if (await _db.StaffInvitations.AsNoTracking().AnyAsync(i =>
                i.RestaurantId == restaurantId &&
                i.NormalizedEmail == normalizedEmail &&
                i.AcceptedAt == null && i.RevokedAt == null &&
                i.ExpiresAt > now, ct))
            throw new InvalidOperationException("An active invitation already exists for this email.");

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var invitation = new StaffInvitation
        {
            RestaurantId = restaurantId,
            Email = email,
            NormalizedEmail = normalizedEmail,
            Role = dto.Role,
            TokenHash = HashToken(token),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            ExpiresAt = now.AddDays(7)
        };
        _db.StaffInvitations.Add(invitation);
        await _db.SaveChangesAsync(ct);
        return new CreatedStaffInvitationDTO
        {
            Id = invitation.Id, Email = email, Role = invitation.Role,
            ExpiresAt = invitation.ExpiresAt, Token = token
        };
    }

    public async Task<StaffMemberDTO> AcceptAsync(
        string actorUserId, string token, CancellationToken ct)
    {
        RequireActor(actorUserId);
        token = (token ?? "").Trim();
        if (token.Length != 64 || !token.All(Uri.IsHexDigit))
            throw new ArgumentException("Invalid invitation token.");
        var hash = HashToken(token.ToUpperInvariant());

        // Serialize concurrent acceptances so quota and one-time token cannot
        // be raced. PostgreSQL may reject a concurrent transaction with 40001.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);
        var invitation = await _db.StaffInvitations
            .SingleOrDefaultAsync(i => i.TokenHash == hash, ct)
            ?? throw new KeyNotFoundException("Invitation not found.");
        var now = DateTimeOffset.UtcNow;
        if (invitation.AcceptedAt != null || invitation.RevokedAt != null ||
            invitation.ExpiresAt <= now)
            throw new InvalidOperationException("Invitation has expired or is no longer valid.");
        ValidateStaffRole(invitation.Role);

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => new { u.Id, u.NormalizedEmail, u.Email, u.FirstName, u.LastName })
            .SingleOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("User not found.");
        if (user.NormalizedEmail != invitation.NormalizedEmail)
            throw new UnauthorizedAccessException("Invitation email does not match your account.");

        var org = await _db.Organizations.Include(o => o.Subscription)
            .SingleOrDefaultAsync(o => o.Restaurants.Any(r =>
                r.Id == invitation.RestaurantId), ct)
            ?? throw new InvalidOperationException("Restaurant has no organization.");
        if (org.OwnerUserId == actorUserId)
            throw new InvalidOperationException("Organization owner does not consume a staff seat.");
        await EnsureEntitledAsync(org, ct);
        var plan = await GetPlanAsync(org, ct);

        if (await _db.RestaurantUsers.AnyAsync(x =>
            x.RestaurantId == invitation.RestaurantId &&
            x.UserId == actorUserId, ct))
            throw new InvalidOperationException("You already belong to this restaurant.");
        var alreadyInOrg = await _db.RestaurantUsers.AsNoTracking()
            .AnyAsync(x => x.Restaurant.OrganizationId == org.Id &&
                           x.UserId == actorUserId, ct);
        var used = await UsedEmployeesAsync(org.Id, org.OwnerUserId, ct);
        if (!alreadyInOrg && used >= plan.MaxEmployees)
            throw new InvalidOperationException("Employee limit reached. Upgrade your plan.");

        var membership = new RestaurantUser
        {
            RestaurantId = invitation.RestaurantId,
            UserId = actorUserId,
            Role = invitation.Role,
            JoinedAt = DateTime.UtcNow
        };
        _db.RestaurantUsers.Add(membership);
        invitation.AcceptedAt = now;
        invitation.AcceptedByUserId = actorUserId;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new StaffMemberDTO
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = membership.Role,
            JoinedAt = membership.JoinedAt
        };
    }

    public async Task<StaffMemberDTO> ChangeRoleAsync(
        Guid restaurantId, string memberUserId, string actorUserId,
        RestaurantRole role, CancellationToken ct)
    {
        ValidateStaffRole(role);
        var org = await RequireOwnerAsync(restaurantId, actorUserId, ct);
        if (memberUserId == org.OwnerUserId)
            throw new InvalidOperationException("Organization owner role cannot be changed here.");
        var member = await _db.RestaurantUsers.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.RestaurantId == restaurantId &&
                                       x.UserId == memberUserId, ct)
            ?? throw new KeyNotFoundException("Employee not found.");
        if (member.Role == RestaurantRole.Owner)
            throw new InvalidOperationException("Owner role cannot be changed here.");
        member.Role = role;
        await _db.SaveChangesAsync(ct);
        return new StaffMemberDTO
        {
            UserId = member.UserId,
            Email = member.User.Email ?? "",
            FirstName = member.User.FirstName,
            LastName = member.User.LastName,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    public async Task RemoveAsync(Guid restaurantId, string memberUserId,
        string actorUserId, CancellationToken ct)
    {
        var org = await RequireOwnerAsync(restaurantId, actorUserId, ct);
        if (memberUserId == org.OwnerUserId)
            throw new InvalidOperationException("Cannot remove the organization owner.");
        var member = await _db.RestaurantUsers.SingleOrDefaultAsync(
            x => x.RestaurantId == restaurantId && x.UserId == memberUserId, ct)
            ?? throw new KeyNotFoundException("Employee not found.");
        if (member.Role == RestaurantRole.Owner)
            throw new InvalidOperationException("Owner membership cannot be removed here.");
        _db.RestaurantUsers.Remove(member);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeInvitationAsync(Guid restaurantId, Guid invitationId,
        string actorUserId, CancellationToken ct)
    {
        await RequireOwnerAsync(restaurantId, actorUserId, ct);
        var invitation = await _db.StaffInvitations.SingleOrDefaultAsync(
            x => x.Id == invitationId && x.RestaurantId == restaurantId, ct)
            ?? throw new KeyNotFoundException("Invitation not found.");
        if (invitation.AcceptedAt != null)
            throw new InvalidOperationException("Accepted invitations cannot be revoked.");
        invitation.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Organization> RequireOwnerAsync(Guid restaurantId,
        string actorUserId, CancellationToken ct)
    {
        RequireActor(actorUserId);
        return await _db.Organizations.Include(x => x.Subscription)
            .SingleOrDefaultAsync(o => o.OwnerUserId == actorUserId &&
                o.Restaurants.Any(r => r.Id == restaurantId), ct)
            ?? throw new KeyNotFoundException("Restaurant or organization not found.");
    }

    private async Task<SubscriptionPlan> GetPlanAsync(Organization org, CancellationToken ct)
    {
        if (org.Subscription == null)
            throw new InvalidOperationException("Organization has no subscription.");
        return await _db.SubscriptionPlans.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == org.Subscription.PlanCode && p.IsActive, ct)
            ?? throw new InvalidOperationException("Subscription plan unavailable.");
    }

    private static Task EnsureEntitledAsync(Organization org, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var s = org.Subscription;
        var now = DateTimeOffset.UtcNow;
        var entitled = s != null &&
            ((s.Status == SubscriptionStatus.Trialing &&
              s.TrialEndsAt is { } trialEnd && trialEnd > now) ||
             ((s.Status == SubscriptionStatus.Active ||
               s.Status == SubscriptionStatus.Canceled) &&
              s.CurrentPeriodEndsAt is { } periodEnd && periodEnd > now));
        if (!entitled) throw new InvalidOperationException("Subscription is not active.");
        return Task.CompletedTask;
    }

    private Task<int> UsedEmployeesAsync(Guid organizationId, string ownerUserId,
        CancellationToken ct) => _db.RestaurantUsers.AsNoTracking()
        .Where(x => x.Restaurant.OrganizationId == organizationId &&
                    x.UserId != ownerUserId)
        .Select(x => x.UserId).Distinct().CountAsync(ct);

    private static void ValidateStaffRole(RestaurantRole role)
    {
        if (role != RestaurantRole.Manager &&
            role != RestaurantRole.Waiter &&
            role != RestaurantRole.Cashier)
            throw new ArgumentException("Staff role must be Manager, Waiter or Cashier.");
    }

    private static void RequireActor(string actorUserId)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
            throw new UnauthorizedAccessException("Authentication required.");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    private static string HashToken(string token) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
