using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantDAL.Repositories
{
    public interface ITableRepository
    {
        Task<IEnumerable<Table>> GetAllAsync(bool includeInactive = false);
        Task<Table?> GetByIdAsync(int id);
        Task<Table?> GetByNumberAsync(int tableNumber);
        Task<bool> TableNumberExistsAsync(int tableNumber, int? excludeId = null);
        Task AddAsync(Table table);
        Task UpdateAsync(Table table);
        Task SaveChangesAsync();
    }

    public class TableRepository : ITableRepository
    {
        private readonly RestaurantDB _context;

        public TableRepository(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Table>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Tables.Where(t => !t.IsDeleted);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            return await query.OrderBy(t => t.TableNumber).ToListAsync();
        }

        public async Task<Table?> GetByIdAsync(int id)
        {
            return await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<Table?> GetByNumberAsync(int tableNumber)
        {
            return await _context.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == tableNumber && !t.IsDeleted);
        }

        public async Task<bool> TableNumberExistsAsync(int tableNumber, int? excludeId = null)
        {
            return await _context.Tables
                .AnyAsync(t => t.TableNumber == tableNumber && !t.IsDeleted && (excludeId == null || t.Id != excludeId));
        }

        public async Task AddAsync(Table table)
        {
            await _context.Tables.AddAsync(table);
        }

        public async Task UpdateAsync(Table table)
        {
            _context.Tables.Update(table);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}