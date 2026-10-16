using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Basket;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public BasketService(
            IBasketRepository basketRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _basketRepository = basketRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<BasketDTO>> GetMyBasketAsync(string userId)
        {
            var items = await _basketRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<BasketDTO>>(items);
        }

        public async Task<BasketDTO> AddToCartAsync(string userId, CreateBasketDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                throw new ArgumentException("Product not found.");
            }

            var existingItem = await _basketRepository.GetByUserAndProductAsync(userId, dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                await _basketRepository.UpdateAsync(existingItem);
                var updatedItem = await _basketRepository.GetByIdAsync(existingItem.Id);
                return _mapper.Map<BasketDTO>(updatedItem);
            }

            var newItem = new Basket
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price
            };

            await _basketRepository.AddAsync(newItem);
            var savedItem = await _basketRepository.GetByIdAsync(newItem.Id);
            return _mapper.Map<BasketDTO>(savedItem);
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int basketItemId, int quantity)
        {
            if (quantity <= 0) return await RemoveFromCartAsync(userId, basketItemId);

            var item = await _basketRepository.GetByIdAsync(basketItemId);
            if (item == null || item.UserId != userId) return false;

            item.Quantity = quantity;
            await _basketRepository.UpdateAsync(item);
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int basketItemId)
        {
            var item = await _basketRepository.GetByIdAsync(basketItemId);
            if (item == null || item.UserId != userId) return false;

            await _basketRepository.DeleteAsync(item);
            return true;
        }

        public async Task ClearCartAsync(string userId)
        {
            await _basketRepository.ClearUserBasketAsync(userId);
        }
    }
}