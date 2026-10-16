using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.ProductFilterDTO
{

    public class ProductFilterDTO
    {
        public bool? Nuts { get; set; }

        public bool? Vegeterian { get; set; }

        public int? Spiciness { get; set; }

        public int? CategoryId { get; set; }
    }

}
