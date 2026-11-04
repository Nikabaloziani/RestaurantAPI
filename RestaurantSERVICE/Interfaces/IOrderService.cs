using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Order;

namespace RestaurantSERVICE.Interfaces;

public interface IOrderService
{
    Task<OrderDTO> CreateOrderFromBasketAsync(Guid restaurantId, string userId, CreateOrderDTO dto, CancellationToken ct = default);
    Task<OrderDTO?> GetOrderByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default);
    Task<List<OrderDTO>> GetMyOrdersAsync(Guid restaurantId, string userId, CancellationToken ct = default);
    Task<List<OrderDTO>> GetAllOrdersAsync(Guid restaurantId, string userId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(Guid restaurantId, int id, OrderStatus status, string userId, CancellationToken ct = default);
}
