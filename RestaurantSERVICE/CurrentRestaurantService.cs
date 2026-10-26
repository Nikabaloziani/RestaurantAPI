
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class CurrentRestaurantService : ICurrentRestaurantService
    {
        private readonly RestaurantDB _context;

        public CurrentRestaurantService(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<bool> HasAccessAsync(
            Guid restaurantId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return await _context.RestaurantUsers
                .AsNoTracking()
                .AnyAsync(
                    x => x.RestaurantId == restaurantId &&
                         x.UserId == userId &&
                         x.Restaurant.IsActive,
                    cancellationToken);
        }
    }
}
