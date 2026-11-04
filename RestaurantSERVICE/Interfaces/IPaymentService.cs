using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;

namespace RestaurantSERVICE.Interfaces;

public interface IPaymentService
{
    Task<PaymentDTO> ProcessPaymentAsync(Guid restaurantId, string userId, CreatePaymentDTO dto, CancellationToken ct = default);
    Task<PaymentDTO?> GetPaymentByIdAsync(Guid restaurantId, string userId, int id, CancellationToken ct = default);
    Task<PaymentDTO?> GetPaymentByOrderIdAsync(Guid restaurantId, string userId, int orderId, CancellationToken ct = default);
    Task<List<PaymentDTO>> GetAllPaymentsAsync(Guid restaurantId, string userId, CancellationToken ct = default);
    Task<bool> UpdatePaymentStatusAsync(Guid restaurantId, string userId, int id, PaymentStatus status, CancellationToken ct = default);
}
