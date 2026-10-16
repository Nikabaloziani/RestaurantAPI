using RestaurantSERVICE.DTO.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantSERVICE.Interfaces
{
    public interface ITableService
    {
        Task<List<TableDTO>> GetAllTablesAsync();
        Task<TableDTO?> GetByIdAsync(int id);
        Task AddAsync(CreateTableDTO dto);
        Task<bool> UpdateStatusAsync(int tableId, bool isOccupied);
        Task DeleteAsync(int id);
    }
}