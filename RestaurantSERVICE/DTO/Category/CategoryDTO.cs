using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Category
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public List<ProductDTO> Products { get; set; } = new();
    }
}
