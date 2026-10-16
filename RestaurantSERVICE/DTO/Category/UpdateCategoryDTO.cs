using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Category
{
    public class UpdateCategoryDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

    }
}
