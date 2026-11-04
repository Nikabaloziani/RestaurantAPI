using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;

namespace RestaurantSERVICE.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllAsync(Guid restaurantId, string userId, CancellationToken ct = default);
        Task<ProductDTO?> GetByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default);
        Task<List<ProductDTO>> GetFilteredAsync(Guid restaurantId, ProductFilterDTO filter, string userId, CancellationToken ct = default);
        Task<ProductDTO> AddAsync(Guid restaurantId, CreateProductDTO dto, string userId, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid restaurantId, UpdateProductDTO dto, string userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default);
    }
}
