using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;

namespace RestaurantDAL.Repositories
{
    public interface IProductRepository : IBaseRepository<Products>
    {
        // Legacy methods retained for other callers; new SaaS routes must NOT use them.
        Task DeleteAsync(int id);
        Task<List<Products>> GetAllWithCategoryAsync();
        Task<Products?> GetByIdWithCategoryAsync(int id);
        Task<List<Products>> GetFilteredAsync(bool? nuts, bool? vegeterian, int? spiciness, int? categoryId);

        Task<List<Products>> GetAllForRestaurantAsync(Guid restaurantId, CancellationToken ct = default);
        Task<Products?> GetByIdForRestaurantAsync(Guid restaurantId, int id, CancellationToken ct = default);
        Task<List<Products>> GetFilteredForRestaurantAsync(Guid restaurantId, bool? nuts, bool? vegeterian, int? spiciness, int? categoryId, CancellationToken ct = default);
    }

    public class ProductRepository : BaseRepository<Products>, IProductRepository
    {
        public ProductRepository(RestaurantDB context) : base(context) { }

        public Task<List<Products>> GetAllWithCategoryAsync() =>
            _dbSet.Include(p => p.Category).AsNoTracking().ToListAsync();

        public Task<Products?> GetByIdWithCategoryAsync(int id) =>
            _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

        public Task<List<Products>> GetAllForRestaurantAsync(Guid restaurantId, CancellationToken ct = default) =>
            _dbSet.AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.RestaurantId == restaurantId && p.Category.RestaurantId == restaurantId)
                .OrderBy(p => p.Id)
                .ToListAsync(ct);

        public Task<Products?> GetByIdForRestaurantAsync(Guid restaurantId, int id, CancellationToken ct = default) =>
            _dbSet.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId && p.Category.RestaurantId == restaurantId, ct);

        public async Task DeleteAsync(int id)
        {
            var product = await _dbSet.FindAsync(id);
            if (product is null) return;
            _dbSet.Remove(product);
            await _context.SaveChangesAsync();
        }

        public Task<List<Products>> GetFilteredAsync(bool? nuts, bool? vegeterian, int? spiciness, int? categoryId) =>
            FilterQuery(_dbSet.Include(p => p.Category), nuts, vegeterian, spiciness, categoryId)
                .AsNoTracking().ToListAsync();

        public Task<List<Products>> GetFilteredForRestaurantAsync(Guid restaurantId, bool? nuts, bool? vegeterian, int? spiciness, int? categoryId, CancellationToken ct = default) =>
            FilterQuery(_dbSet.Include(p => p.Category)
                    .Where(p => p.RestaurantId == restaurantId && p.Category.RestaurantId == restaurantId),
                nuts, vegeterian, spiciness, categoryId)
                .AsNoTracking().ToListAsync(ct);

        private static IQueryable<Products> FilterQuery(IQueryable<Products> query, bool? nuts, bool? vegeterian, int? spiciness, int? categoryId)
        {
            if (nuts.HasValue) query = query.Where(p => p.Nuts == nuts.Value);
            if (vegeterian.HasValue) query = query.Where(p => p.Vegeterian == vegeterian.Value);
            if (spiciness.HasValue) query = query.Where(p => p.Spiciness == spiciness.Value);
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            return query;
        }
    }
}
