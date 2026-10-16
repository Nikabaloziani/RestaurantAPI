using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantDATA.Infrastructures
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly RestaurantDB _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(RestaurantDB context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }//

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}