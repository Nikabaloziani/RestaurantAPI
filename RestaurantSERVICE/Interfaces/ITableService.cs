
using RestaurantSERVICE.DTO.Table;

namespace RestaurantSERVICE.Interfaces
{
    public interface ITableService
    {
        Task<List<TableDTO>> GetAllTablesAsync(Guid restaurantId,string userId,CancellationToken cancellationToken = default);

        Task<TableDTO?> GetByIdAsync(Guid restaurantId,int id,string userId, CancellationToken cancellationToken = default);

        Task AddAsync(Guid restaurantId, CreateTableDTO dto,string userId,CancellationToken cancellationToken = default);

        Task<bool> UpdateDetailsAsync(Guid restaurantId,int id,EditTableDTO dto,string userId,CancellationToken cancellationToken = default);

        Task<bool> UpdateStatusAsync(Guid restaurantId,int tableId,bool isOccupied,string userId,CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid restaurantId, int id,string userId,CancellationToken cancellationToken = default);
    }
}
