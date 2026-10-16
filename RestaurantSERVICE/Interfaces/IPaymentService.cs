using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO dto);
        Task<PaymentDTO?> GetPaymentByIdAsync(int id);
        Task<PaymentDTO?> GetPaymentByOrderIdAsync(int orderId);
        Task<List<PaymentDTO>> GetAllPaymentsAsync();
        Task<bool> UpdatePaymentStatusAsync(int id, PaymentStatus status);
    }
}
