using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;

namespace RestaurantDAL.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<List<Order>> GetAllAsync();
        Task UpdateAsync(Order order);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly RestaurantDB _context;

        public OrderRepository(RestaurantDB context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}