using RestaurantSERVICE.DTO.Category; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantSERVICE.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO?> GetByIdAsync(int id);
        Task AddAsync(CreateCategoryDTO dto);
        Task<bool> UpdateAsync(int id, UpdateCategoryDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}