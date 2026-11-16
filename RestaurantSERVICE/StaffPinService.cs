using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.StaffPin;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE.Services;

/// <summary>
/// Self-service PIN setup/rotation/disable after verifying the ACCOUNT PASSWORD.
/// Does NOT authenticate by PIN and does NOT issue JWTs or device credentials.
/// </summary>
public sealed class StaffPinService : IStaffPinService
{
    private readonly RestaurantDB _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IStaffPinHasher _pinHasher;

    public StaffPinService(
        RestaurantDB db,
        UserManager<ApplicationUser> users,
        IStaffPinHasher pinHasher)
    {
        _db = db;
        _users = users;
        _pinHasher = pinHasher;
    }

    public async Task<MyPinStatusDTO> GetMyStatusAsync(
        Guid restaurantId, string userId, CancellationToken ct)
    {
        var organizationId = await RequireMembershipAsync(restaurantId, userId, ct);
        var credential = await _db.StaffPinCredentials.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId &&
                                       x.UserId == userId, ct);
        return ToStatus(credential);
    }

    public async Task<MyPinStatusDTO> SetMyPinAsync(
        Guid restaurantId, string userId, SetMyPinDTO dto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidatePin(dto.NewPin);
        var organizationId = await RequireMembershipAsync(restaurantId, userId, ct);
        await ConfirmAccountPasswordAsync(userId, dto.CurrentPassword, ct);

        var now = DateTimeOffset.UtcNow;
        var credential = await _db.StaffPinCredentials.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId, ct);

        if (credential == null)
        {
            credential = new StaffPinCredential
            {
                OrganizationId = organizationId,
                UserId = userId,
                CreatedAt = now,
                PepperKeyVersion = 1
            };
            _db.StaffPinCredentials.Add(credential);
        }
        else
        {
            credential.PepperKeyVersion = 1;
        }

        credential.PinHash = _pinHasher.Hash(credential, dto.NewPin);
        credential.IsEnabled = true;
        credential.FailedAttemptCount = 0;
        credential.LockedUntil = null;
        credential.LastUsedAt = null;
        credential.UpdatedAt = now;

        // Changing/resetting the PIN revokes any server-tracked staff sessions.
        await RevokeStaffSessionsAsync(organizationId, userId, now, ct);
        AddAudit(organizationId, restaurantId, userId, "pin.set", "success", now);
        await _db.SaveChangesAsync(ct);
        return ToStatus(credential);
    }

    public async Task DisableMyPinAsync(
        Guid restaurantId, string userId, DisableMyPinDTO dto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var organizationId = await RequireMembershipAsync(restaurantId, userId, ct);
        await ConfirmAccountPasswordAsync(userId, dto.CurrentPassword, ct);

        var credential = await _db.StaffPinCredentials.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId, ct);
        var now = DateTimeOffset.UtcNow;
        if (credential != null)
        {
            credential.IsEnabled = false;
            // Discard the old verifier; a future PIN must be set with password confirmation.
            credential.PinHash = string.Empty;
            credential.FailedAttemptCount = 0;
            credential.LockedUntil = null;
            credential.UpdatedAt = now;
        }

        await RevokeStaffSessionsAsync(organizationId, userId, now, ct);
        AddAudit(organizationId, restaurantId, userId, "pin.disable", "success", now);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Guid> RequireMembershipAsync(
        Guid restaurantId, string userId, CancellationToken ct)
    {
        if (restaurantId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Access denied.");

        var membership = await _db.RestaurantUsers.AsNoTracking()
            .Where(ru => ru.RestaurantId == restaurantId &&
                         ru.UserId == userId &&
                         ru.Restaurant.IsActive)
            .Select(ru => new
            {
                OrganizationId = ru.Restaurant.OrganizationId,
                ru.Role
            })
            .SingleOrDefaultAsync(ct);

        if (membership?.OrganizationId is not Guid organizationId)
            throw new KeyNotFoundException(
                "No active organization-linked restaurant membership found.");

        // PIN mode is for staff. Owners continue to use strong account login.
        if (membership.Role == RestaurantRole.Owner)
            throw new UnauthorizedAccessException("Owner accounts cannot use staff PIN mode.");

        return organizationId;
    }

    private async Task ConfirmAccountPasswordAsync(
        string userId, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Account password is required.");

        ct.ThrowIfCancellationRequested();
        var user = await _users.FindByIdAsync(userId);
        if (user == null || await _users.IsLockedOutAsync(user) ||
            !await _users.CheckPasswordAsync(user, password))
        {
            // Do not reveal which part of credential verification failed.
            throw new UnauthorizedAccessException("Account verification failed.");
        }
    }

    private async Task RevokeStaffSessionsAsync(
        Guid organizationId, string userId, DateTimeOffset now, CancellationToken ct)
    {
        var sessions = await _db.StaffDeviceSessions
            .Where(s => s.OrganizationId == organizationId &&
                        s.UserId == userId && s.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var session in sessions)
            session.RevokedAt = now;
    }

    private void AddAudit(Guid organizationId, Guid restaurantId,
        string userId, string action, string outcome, DateTimeOffset now)
    {
        _db.StaffSecurityAuditEvents.Add(new StaffSecurityAuditEvent
        {
            OrganizationId = organizationId,
            RestaurantId = restaurantId,
            ActorUserId = userId,
            Action = action,
            Outcome = outcome,
            OccurredAt = now
        });
    }

    private static MyPinStatusDTO ToStatus(StaffPinCredential? credential) =>
        new()
        {
            HasPin = credential != null && !string.IsNullOrWhiteSpace(credential.PinHash),
            IsEnabled = credential?.IsEnabled == true &&
                        !string.IsNullOrWhiteSpace(credential.PinHash),
            UpdatedAt = credential?.UpdatedAt
        };

    private static void ValidatePin(string? pin)
    {
        if (pin == null || pin.Length != 6 || pin.Any(c => c is < '0' or > '9'))
            throw new ArgumentException("PIN must contain exactly 6 digits (0-9).");

        // Small starter blocklist; expand with common PINs before production.
        if (pin.Distinct().Count() == 1 ||
            pin is "123456" or "654321" or "012345" or "987654" or
                   "111222" or "121212" or "123123" or "112233" or "000123")
            throw new ArgumentException("PIN is too common. Choose a less predictable PIN.");
    }
}
