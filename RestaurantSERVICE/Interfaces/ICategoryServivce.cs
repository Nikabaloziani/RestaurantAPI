using RestaurantSERVICE.DTO.Category;

namespace RestaurantSERVICE.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllAsync(Guid restaurantId, string userId, CancellationToken ct = default);
        Task<CategoryDTO?> GetByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default);
        Task AddAsync(Guid restaurantId, CreateCategoryDTO dto, string userId, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid restaurantId, int id, UpdateCategoryDTO dto, string userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default);
    }
}
