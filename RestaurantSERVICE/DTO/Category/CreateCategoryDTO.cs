using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Category
{
    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }

    }
}
