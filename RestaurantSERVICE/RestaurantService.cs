using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE
{

    using Microsoft.EntityFrameworkCore;
    using RestaurantDATA.DB_Context;
    using RestaurantDATA.Entities;
    using RestaurantDATA.Enum;
    using RestaurantSERVICE.DTO.Restaurant;
    using RestaurantSERVICE.Interfaces;
    using System.Text.RegularExpressions;

    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDB _context;

        public RestaurantService(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<RestaurantResponseDTO> CreateAsync(
            CreateRestaurantDTO dto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            var name = dto.Name?.Trim() ?? string.Empty;
            var slug = dto.Slug?.Trim().ToLowerInvariant()
                ?? string.Empty;

            if (name.Length < 2 || name.Length > 150)
                throw new ArgumentException(
                    "Restaurant name must contain 2-150 characters.");

            if (slug.Length > 150 ||
                !Regex.IsMatch(
                    slug,
                    @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            {
                throw new ArgumentException(
                    "Invalid restaurant slug.");
            }

            var slugExists = await _context.Restaurants
                .AnyAsync(
                    x => x.Slug == slug,
                    cancellationToken);

            if (slugExists)
                throw new InvalidOperationException(
                    "Restaurant slug already exists.");

            var restaurantId = Guid.NewGuid();

            var restaurant = new Restaurant
            {
                Id = restaurantId,
                Name = name,
                Slug = slug,
                Code = "REST-" +
                    restaurantId.ToString("N")[..16].ToUpperInvariant(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var owner = new RestaurantUser
            {
                RestaurantId = restaurantId,
                UserId = userId,
                Role = RestaurantRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _context.Restaurants.Add(restaurant);
            _context.RestaurantUsers.Add(owner);

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDTO(restaurant, owner.Role);
        }

        public async Task<List<RestaurantResponseDTO>> GetMineAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            return await _context.RestaurantUsers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Restaurant.Name)
                .Select(x => new RestaurantResponseDTO
                {
                    Id = x.Restaurant.Id,
                    Code = x.Restaurant.Code,
                    Name = x.Restaurant.Name,
                    Slug = x.Restaurant.Slug,
                    IsActive = x.Restaurant.IsActive,
                    CreatedAt = x.Restaurant.CreatedAt,
                    Role = x.Role
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<RestaurantResponseDTO?> GetByIdAsync(
            Guid restaurantId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            return await _context.RestaurantUsers
                .AsNoTracking()
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    x.UserId == userId)
                .Select(x => new RestaurantResponseDTO
                {
                    Id = x.Restaurant.Id,
                    Code = x.Restaurant.Code,
                    Name = x.Restaurant.Name,
                    Slug = x.Restaurant.Slug,
                    IsActive = x.Restaurant.IsActive,
                    CreatedAt = x.Restaurant.CreatedAt,
                    Role = x.Role
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RestaurantResponseDTO> UpdateAsync(
            Guid restaurantId,
            UpdateRestaurantDTO dto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            var name = dto.Name?.Trim() ?? string.Empty;

            if (name.Length < 2 || name.Length > 150)
                throw new ArgumentException(
                    "Restaurant name must contain 2-150 characters.");

            var membership = await _context.RestaurantUsers
                .Include(x => x.Restaurant)
                .FirstOrDefaultAsync(
                    x => x.RestaurantId == restaurantId &&
                         x.UserId == userId,
                    cancellationToken);

            if (membership == null)
                throw new KeyNotFoundException(
                    "Restaurant not found.");

            if (membership.Role != RestaurantRole.Owner)
                throw new UnauthorizedAccessException(
                    "Only the owner can update this restaurant.");

            var restaurant = membership.Restaurant;

            if (!restaurant.IsActive)
                throw new InvalidOperationException(
                    "Restaurant is inactive.");

            restaurant.Name = name;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDTO(restaurant, membership.Role);
        }

        private static RestaurantResponseDTO MapToDTO(
            Restaurant restaurant,
            RestaurantRole role)
        {
            return new RestaurantResponseDTO
            {
                Id = restaurant.Id,
                Code = restaurant.Code,
                Name = restaurant.Name,
                Slug = restaurant.Slug,
                IsActive = restaurant.IsActive,
                CreatedAt = restaurant.CreatedAt,
                Role = role
            };
        }
    }

}
