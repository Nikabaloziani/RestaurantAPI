using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;

namespace RestaurantDAL.Repositories
{
    public interface IProductRepository : IBaseRepository<Products>
    {
        Task DeleteAsync(int id);
        Task<List<Products>> GetAllWithCategoryAsync();
        Task<Products?> GetByIdWithCategoryAsync(int id);
        Task<List<Products>> GetFilteredAsync(bool? nuts, bool? vegeterian, int? spiciness, int? categoryId);
    }

    public class ProductRepository : BaseRepository<Products>, IProductRepository
    {
        public ProductRepository(RestaurantDB context) : base(context) { }

        public async Task<List<Products>> GetAllWithCategoryAsync()
        {
            return await _dbSet.Include(x => x.Category).AsNoTracking().ToListAsync();
        }

        public async Task<Products?> GetByIdWithCategoryAsync(int id)
        {
            return await _dbSet.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _dbSet.FindAsync(id);
            if (product != null)
            {
                _dbSet.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Products>> GetFilteredAsync(bool? nuts, bool? vegeterian, int? spiciness, int? categoryId)
        {
            var query = _dbSet.Include(x => x.Category).AsQueryable();

            if (nuts.HasValue)
                query = query.Where(x => x.Nuts == nuts.Value);

            if (vegeterian.HasValue)
                query = query.Where(x => x.Vegeterian == vegeterian.Value);

            if (spiciness.HasValue)
                query = query.Where(x => x.Spiciness == spiciness.Value);

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId.Value);

            return await query.AsNoTracking().ToListAsync();
        }
    }
}