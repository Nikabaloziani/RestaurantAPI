using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly RestaurantDB _context;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository products, ICategoryRepository categories,
            RestaurantDB context, IMapper mapper)
        {
            _products = products;
            _categories = categories;
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductDTO>> GetAllAsync(Guid restaurantId, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: false, ct);
            return _mapper.Map<List<ProductDTO>>(await _products.GetAllForRestaurantAsync(restaurantId, ct));
        }

        public async Task<ProductDTO?> GetByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: false, ct);
            var product = await _products.GetByIdForRestaurantAsync(restaurantId, id, ct);
            return product is null ? null : _mapper.Map<ProductDTO>(product);
        }

        public async Task<List<ProductDTO>> GetFilteredAsync(Guid restaurantId, ProductFilterDTO filter, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: false, ct);
            var rows = await _products.GetFilteredForRestaurantAsync(
                restaurantId, filter.Nuts, filter.Vegeterian, filter.Spiciness, filter.CategoryId, ct);
            return _mapper.Map<List<ProductDTO>>(rows);
        }

        public async Task<ProductDTO> AddAsync(Guid restaurantId, CreateProductDTO dto, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var category = await _categories.GetByIdForRestaurantAsync(restaurantId, dto.CategoryId, ct: ct);
            if (category is null) throw new ArgumentException("Category does not belong to this restaurant.");
            var product = _mapper.Map<Products>(dto);
            product.RestaurantId = restaurantId;
            await _products.AddAsync(product);
            // Ensure CategoryName is populated if the DTO expects it.
            var saved = await _products.GetByIdForRestaurantAsync(restaurantId, product.Id, ct)
                ?? throw new InvalidOperationException("Created product could not be loaded.");
            return _mapper.Map<ProductDTO>(saved);
        }

        public async Task<bool> UpdateAsync(Guid restaurantId, UpdateProductDTO dto, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var existing = await _products.GetByIdForRestaurantAsync(restaurantId, dto.Id, ct);
            if (existing is null) return false;
            var category = await _categories.GetByIdForRestaurantAsync(restaurantId, dto.CategoryId, ct: ct);
            if (category is null) throw new ArgumentException("Category does not belong to this restaurant.");
            _mapper.Map(dto, existing);
            existing.RestaurantId = restaurantId;
            await _products.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var existing = await _products.GetByIdForRestaurantAsync(restaurantId, id, ct);
            if (existing is null) return false;
            await _products.DeleteAsync(existing);
            return true;
        }
    }
}
