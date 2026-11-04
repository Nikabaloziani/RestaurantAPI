using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;

namespace RestaurantDAL.Repositories
{
    public interface ICategoryRepository
    {
        // Legacy methods retained for other existing callers. Do not expose these on SaaS HTTP routes.
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id, bool includeProducts = false);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);

        // Tenant-scoped methods for the new catalog endpoints.
        Task<List<Category>> GetAllForRestaurantAsync(Guid restaurantId, CancellationToken ct = default);
        Task<Category?> GetByIdForRestaurantAsync(Guid restaurantId, int id, bool includeProducts = false, CancellationToken ct = default);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly RestaurantDB _context;
        public CategoryRepository(RestaurantDB context) => _context = context;

        public Task<List<Category>> GetAllAsync() => _context.Categories
            .Include(c => c.Products).AsNoTracking().ToListAsync();

        public Task<Category?> GetByIdAsync(int id, bool includeProducts = false)
        {
            IQueryable<Category> query = _context.Categories;
            if (includeProducts) query = query.Include(c => c.Products);
            return query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<List<Category>> GetAllForRestaurantAsync(Guid restaurantId, CancellationToken ct = default) =>
            _context.Categories
                .AsNoTracking()
                .Where(c => c.RestaurantId == restaurantId)
                .Include(c => c.Products.Where(p => p.RestaurantId == restaurantId))
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

        public Task<Category?> GetByIdForRestaurantAsync(Guid restaurantId, int id, bool includeProducts = false, CancellationToken ct = default)
        {
            IQueryable<Category> query = _context.Categories.Where(c => c.RestaurantId == restaurantId);
            if (includeProducts)
                query = query.Include(c => c.Products.Where(p => p.RestaurantId == restaurantId));
            return query.FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
