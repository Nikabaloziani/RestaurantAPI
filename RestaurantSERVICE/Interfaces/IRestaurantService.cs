using RestaurantSERVICE.DTO.Restaurant;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{

    public interface IRestaurantService
    {
        Task<RestaurantResponseDTO> CreateAsync(
            CreateRestaurantDTO dto,
            string userId,
            CancellationToken cancellationToken = default
        );

        Task<List<RestaurantResponseDTO>> GetMineAsync(
            string userId,
            CancellationToken cancellationToken = default
        );

        Task<RestaurantResponseDTO?> GetByIdAsync(
            Guid restaurantId,
            string userId,
            CancellationToken cancellationToken = default
        );

        Task<RestaurantResponseDTO> UpdateAsync(
            Guid restaurantId,
            UpdateRestaurantDTO dto,
            string userId,
            CancellationToken cancellationToken = default
        );
    }

}
