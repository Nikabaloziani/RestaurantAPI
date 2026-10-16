using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;

namespace RestaurantDAL.Repositories
{
    public interface IBasketRepository
    {
        Task<List<Basket>> GetByUserIdAsync(string userId);
        Task<Basket?> GetByIdAsync(int id);
        Task<Basket?> GetByUserAndProductAsync(string userId, int productId);
        Task AddAsync(Basket basket);
        Task UpdateAsync(Basket basket);
        Task DeleteAsync(Basket basket);
        Task ClearUserBasketAsync(string userId);
    }

    public class BasketRepository : IBasketRepository
    {
        private readonly RestaurantDB _context;

        public BasketRepository(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<List<Basket>> GetByUserIdAsync(string userId)
        {
            return await _context.Baskets
                .Include(b => b.Product)
                .Where(b => b.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Basket?> GetByIdAsync(int id)
        {
            return await _context.Baskets
                .Include(b => b.Product)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Basket?> GetByUserAndProductAsync(string userId, int productId)
        {
            return await _context.Baskets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ProductId == productId);
        }

        public async Task AddAsync(Basket basket)
        {
            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Basket basket)
        {
            _context.Baskets.Update(basket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Basket basket)
        {
            _context.Baskets.Remove(basket);
            await _context.SaveChangesAsync();
        }

        public async Task ClearUserBasketAsync(string userId)
        {
            var userItems = await _context.Baskets.Where(b => b.UserId == userId).ToListAsync();
            if (userItems.Any())
            {
                _context.Baskets.RemoveRange(userItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}