using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDTO> CreateOrderFromBasketAsync(string userId, CreateOrderDTO dto);
        Task<OrderDTO?> GetOrderByIdAsync(int id);
        Task<List<OrderDTO>> GetMyOrdersAsync(string userId);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<bool> UpdateStatusAsync(int id, OrderStatus status);
    }
}
