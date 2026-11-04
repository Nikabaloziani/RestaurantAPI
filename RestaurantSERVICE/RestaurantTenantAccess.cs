using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Enum;

namespace RestaurantSERVICE;

// Centralized authorization for tenant-scoped orders, baskets, payments and reports.
// ASP.NET Identity's global Admin/Waiter role does not imply membership in a restaurant.
internal static class RestaurantTenantAccess
{
    public static async Task EnsureAsync(
        RestaurantDB context, Guid restaurantId, string userId,
        CancellationToken ct, params RestaurantRole[] allowedRoles)
    {
        if (restaurantId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Restaurant access denied.");

        var role = await context.RestaurantUsers.AsNoTracking()
            .Where(x => x.RestaurantId == restaurantId && x.UserId == userId && x.Restaurant.IsActive)
            .Select(x => (RestaurantRole?)x.Role)
            .SingleOrDefaultAsync(ct);

        if (!role.HasValue || !allowedRoles.Contains(role.Value))
            throw new UnauthorizedAccessException("Restaurant access denied.");
    }
}
