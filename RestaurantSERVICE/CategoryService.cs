using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDAL.Repositories;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Category;
using RestaurantSERVICE.Services;

namespace RestaurantSERVICE
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly RestaurantDB _context;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repository, RestaurantDB context, IMapper mapper)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CategoryDTO>> GetAllAsync(Guid restaurantId, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: false, ct);
            var categories = await _repository.GetAllForRestaurantAsync(restaurantId, ct);
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: false, ct);
            var category = await _repository.GetByIdForRestaurantAsync(restaurantId, id, includeProducts: true, ct);
            return category is null ? null : _mapper.Map<CategoryDTO>(category);
        }

        public async Task AddAsync(Guid restaurantId, CreateCategoryDTO dto, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var entity = _mapper.Map<Category>(dto);
            entity.RestaurantId = restaurantId;
            await _repository.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Guid restaurantId, int id, UpdateCategoryDTO dto, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var existing = await _repository.GetByIdForRestaurantAsync(restaurantId, id, ct: ct);
            if (existing is null) return false;
            _mapper.Map(dto, existing);
            existing.RestaurantId = restaurantId;
            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default)
        {
            await CatalogTenantAccess.EnsureAsync(_context, restaurantId, userId, write: true, ct);
            var existing = await _repository.GetByIdForRestaurantAsync(restaurantId, id, ct: ct);
            if (existing is null) return false;
            // Check all referencing products, including legacy records, to prevent FK failures.
            if (await _context.Products.AnyAsync(p => p.CategoryId == id, ct))
                throw new InvalidOperationException("Category contains products.");
            await _repository.DeleteAsync(existing);
            return true;
        }
    }
}
