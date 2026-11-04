using RestaurantSERVICE.DTO.Basket;

namespace RestaurantSERVICE.Interfaces;

public interface IBasketService
{
    Task<List<BasketDTO>> GetMyBasketAsync(Guid restaurantId, string userId, CancellationToken ct = default);
    Task<BasketDTO> AddToCartAsync(Guid restaurantId, string userId, CreateBasketDTO dto, CancellationToken ct = default);
    Task<bool> UpdateQuantityAsync(Guid restaurantId, string userId, int itemId, int quantity, CancellationToken ct = default);
    Task<bool> RemoveFromCartAsync(Guid restaurantId, string userId, int itemId, CancellationToken ct = default);
    Task ClearCartAsync(Guid restaurantId, string userId, CancellationToken ct = default);
}
