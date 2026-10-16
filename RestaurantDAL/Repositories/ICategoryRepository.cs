using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;

namespace RestaurantDAL.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id, bool includeProducts = false);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly RestaurantDB _context;

        public CategoryRepository(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id, bool includeProducts = false)
        {
            IQueryable<Category> query = _context.Categories;

            if (includeProducts)
            {
                query = query.Include(c => c.Products);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id);
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