using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<List<ProductDTO>> GetFilteredAsync(ProductFilterDTO filter);
        Task<ProductDTO> AddAsync(CreateProductDTO dto);
        Task<bool> UpdateAsync(UpdateProductDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
