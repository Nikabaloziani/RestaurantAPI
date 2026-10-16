using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Basket;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IBasketService
    {
        Task<List<BasketDTO>> GetMyBasketAsync(string userId);
        Task<BasketDTO> AddToCartAsync(string userId, CreateBasketDTO dto);
        Task<bool> UpdateQuantityAsync(string userId, int basketItemId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int basketItemId);
        Task ClearCartAsync(string userId);
    }
}
