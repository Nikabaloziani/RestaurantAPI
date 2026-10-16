using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Product
{
    public class UpdateProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Nuts { get; set; }
        public bool Vegeterian { get; set; }
        public int Spiciness { get; set; }
        public int CategoryId { get; set; }
    }
}
