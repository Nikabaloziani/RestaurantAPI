using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Category;
using RestaurantSERVICE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantSERVICE
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task AddAsync(CreateCategoryDTO dto)
        {
            var categoryEntity = _mapper.Map<Category>(dto);
            await _categoryRepository.AddAsync(categoryEntity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null) return false;

            _mapper.Map(dto, existingCategory);
            await _categoryRepository.UpdateAsync(existingCategory);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null) return false;

            await _categoryRepository.DeleteAsync(existingCategory);
            return true;
        }
    }
}