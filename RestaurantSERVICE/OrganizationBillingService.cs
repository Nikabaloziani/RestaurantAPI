using System.Data;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Billing;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE.Services;

public sealed class OrganizationBillingService : IOrganizationBillingService
{
    private readonly RestaurantDB _db;
    public OrganizationBillingService(RestaurantDB db) => _db = db;

    public Task<List<PlanDTO>> GetPlansAsync(CancellationToken ct) =>
        _db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.MonthlyPriceUsd)
            .Select(x => new PlanDTO
            {
                Code = x.Code,
                Name = x.Name,
                MonthlyPriceUsd = x.MonthlyPriceUsd,
                MaxLocations = x.MaxLocations,
                MaxEmployees = x.MaxEmployees,
                ReservationsEnabled = x.ReservationsEnabled,
                BasicAnalyticsEnabled = x.BasicAnalyticsEnabled,
                AdvancedAnalyticsEnabled = x.AdvancedAnalyticsEnabled,
                QrMenuEnabled = x.QrMenuEnabled,
                KitchenDisplayEnabled = x.KitchenDisplayEnabled,
                InventoryEnabled = x.InventoryEnabled,
                MultiBranchAnalyticsEnabled = x.MultiBranchAnalyticsEnabled
            }).ToListAsync(ct);

    public async Task<OrganizationDTO> CreateOrganizationAsync(
        string userId, string name, CancellationToken ct)
    {
        RequireUser(userId);
        name = (name ?? string.Empty).Trim();
        if (name.Length is < 2 or > 150)
            throw new ArgumentException("Organization name must contain 2–150 characters.");

        if (await _db.Organizations.AnyAsync(x => x.OwnerUserId == userId, ct))
            throw new InvalidOperationException("You already own an organization.");

        var now = DateTimeOffset.UtcNow;
        var organization = new Organization
        {
            Name = name,
            OwnerUserId = userId,
            CreatedAt = now,
            Subscription = new Subscription
            {
                PlanCode = "starter",
                Status = SubscriptionStatus.Trialing,
                StartedAt = now,
                TrialEndsAt = now.AddDays(14)
            }
        };

        _db.Organizations.Add(organization);
        // A unique database index also protects one-organization-per-owner
        // against simultaneous requests.
        await _db.SaveChangesAsync(ct);
        return new OrganizationDTO { Id = organization.Id, Name = organization.Name };
    }

    public Task<List<OrganizationDTO>> GetMineAsync(string userId, CancellationToken ct)
    {
        RequireUser(userId);
        return _db.Organizations.AsNoTracking()
            .Where(x => x.OwnerUserId == userId)
            .Select(x => new OrganizationDTO
            {
                Id = x.Id,
                Name = x.Name,
                LocationCount = x.Restaurants.Count
            }).ToListAsync(ct);
    }

    public async Task<SubscriptionDTO?> GetSubscriptionAsync(
        Guid organizationId, string userId, CancellationToken ct)
    {
        RequireUser(userId);
        var org = await _db.Organizations.AsNoTracking()
            .Where(x => x.Id == organizationId && x.OwnerUserId == userId)
            .Select(x => new
            {
                x.OwnerUserId,
                x.Id,
                Subscription = x.Subscription,
                LocationCount = x.Restaurants.Count
            }).SingleOrDefaultAsync(ct);
        if (org?.Subscription == null) return null;

        // Plan is loaded explicitly; do not trust any client-supplied limits.
        var plan = await _db.SubscriptionPlans.AsNoTracking()
            .SingleAsync(x => x.Code == org.Subscription.PlanCode, ct);
        var usedEmployees = await _db.RestaurantUsers.AsNoTracking()
            .Where(x => x.Restaurant.OrganizationId == organizationId &&
                        x.UserId != org.OwnerUserId)
            .Select(x => x.UserId).Distinct().CountAsync(ct);

        return new SubscriptionDTO
        {
            OrganizationId = org.Id,
            PlanCode = plan.Code,
            Status = org.Subscription.Status,
            TrialEndsAt = org.Subscription.TrialEndsAt,
            CurrentPeriodEndsAt = org.Subscription.CurrentPeriodEndsAt,
            MaxLocations = plan.MaxLocations,
            UsedLocations = org.LocationCount,
            MaxEmployees = plan.MaxEmployees,
            UsedEmployees = usedEmployees,
            IsCurrentlyEntitled = IsEntitled(org.Subscription, DateTimeOffset.UtcNow)
        };
    }

    public async Task<OrganizationDTO> AttachExistingRestaurantAsync(
        Guid organizationId, Guid restaurantId, string userId, CancellationToken ct)
    {
        RequireUser(userId);
        // Serializes quota checks against simultaneous attachment requests.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var organization = await _db.Organizations
            .Include(x => x.Subscription)
            .SingleOrDefaultAsync(x => x.Id == organizationId &&
                                       x.OwnerUserId == userId, ct);
        if (organization == null)
            throw new KeyNotFoundException("Organization not found.");
        if (organization.Subscription == null)
            throw new InvalidOperationException("Organization has no subscription.");
        if (!IsEntitled(organization.Subscription, DateTimeOffset.UtcNow))
            throw new InvalidOperationException("Subscription is not active.");

        var isRestaurantOwner = await _db.RestaurantUsers.AsNoTracking()
            .AnyAsync(x => x.RestaurantId == restaurantId &&
                           x.UserId == userId && x.Role == RestaurantRole.Owner, ct);
        if (!isRestaurantOwner)
            throw new KeyNotFoundException("Restaurant not found.");

        var restaurant = await _db.Restaurants
            .SingleAsync(x => x.Id == restaurantId, ct);
        if (restaurant.OrganizationId == organizationId)
        {
            await transaction.CommitAsync(ct);
            return new OrganizationDTO { Id = organization.Id, Name = organization.Name,
                LocationCount = await _db.Restaurants.CountAsync(
                    x => x.OrganizationId == organizationId, ct) };
        }
        if (restaurant.OrganizationId != null)
            throw new InvalidOperationException("Restaurant belongs to another organization.");

        var plan = await _db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.Code == organization.Subscription.PlanCode && x.IsActive)
            .Select(x => new { x.MaxLocations, x.MaxEmployees })
            .SingleOrDefaultAsync(ct);
        if (plan == null)
            throw new InvalidOperationException("Subscription plan is unavailable.");

        var currentCount = await _db.Restaurants
            .CountAsync(x => x.OrganizationId == organizationId, ct);
        if (currentCount >= plan.MaxLocations)
            throw new InvalidOperationException("Location limit reached. Upgrade your plan.");

        // An existing restaurant may already have employees. Attaching it must
        // not bypass the organization's staff quota. The owner is excluded,
        // matching GetSubscriptionAsync's existing UsedEmployees definition.
        var currentEmployeeIds = await _db.RestaurantUsers.AsNoTracking()
            .Where(x => x.Restaurant.OrganizationId == organizationId &&
                        x.UserId != organization.OwnerUserId)
            .Select(x => x.UserId).Distinct().ToListAsync(ct);
        var incomingEmployeeIds = await _db.RestaurantUsers.AsNoTracking()
            .Where(x => x.RestaurantId == restaurantId &&
                        x.UserId != organization.OwnerUserId)
            .Select(x => x.UserId).Distinct().ToListAsync(ct);
        var uniqueEmployeeCount = currentEmployeeIds
            .Concat(incomingEmployeeIds)
            .Distinct(StringComparer.Ordinal)
            .Count();
        if (uniqueEmployeeCount > plan.MaxEmployees)
            throw new InvalidOperationException("Employee limit reached. Upgrade your plan.");

        restaurant.OrganizationId = organizationId;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new OrganizationDTO { Id = organization.Id, Name = organization.Name,
            LocationCount = currentCount + 1 };
    }

    private static bool IsEntitled(Subscription subscription, DateTimeOffset now) =>
        (subscription.Status == SubscriptionStatus.Trialing &&
         subscription.TrialEndsAt is { } trialEnd && trialEnd > now) ||
        ((subscription.Status == SubscriptionStatus.Active ||
          subscription.Status == SubscriptionStatus.Canceled) &&
         subscription.CurrentPeriodEndsAt is { } periodEnd && periodEnd > now);

    private static void RequireUser(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Authentication required.");
    }
}
