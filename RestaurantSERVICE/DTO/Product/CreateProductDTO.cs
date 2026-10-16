using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Product
{
    public class CreateProductDTO
    {
        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public bool Nuts { get; set; }

        public bool Vegeterian { get; set; }

        public int Spiciness { get; set; }

        public int CategoryId { get; set; }
    }
}
