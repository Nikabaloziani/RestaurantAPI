using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Enum;

namespace RestaurantSERVICE
{
    // Shared membership + role check. Never trust restaurantId without checking this.
    internal static class CatalogTenantAccess
    {
        public static async Task EnsureAsync(
            RestaurantDB context, Guid restaurantId, string userId,
            bool write, CancellationToken ct)
        {
            if (restaurantId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Restaurant access denied.");

            var role = await context.RestaurantUsers
                .AsNoTracking()
                .Where(ru => ru.RestaurantId == restaurantId &&
                             ru.UserId == userId && ru.Restaurant.IsActive)
                .Select(ru => (RestaurantRole?)ru.Role)
                .SingleOrDefaultAsync(ct);

            bool allowed = role switch
            {
                RestaurantRole.Owner => true,
                RestaurantRole.Manager => true,
                RestaurantRole.Waiter => !write,
                RestaurantRole.Cashier => !write,
                _ => false,
            };
            if (!allowed)
                throw new UnauthorizedAccessException("Restaurant access denied.");
        }
    }
}
