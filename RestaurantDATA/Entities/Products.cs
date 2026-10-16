using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{
    [Table("Products")]
    public class Products
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public bool Nuts { get; set; }
        public bool Vegeterian { get; set; }
        public int Spiciness { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<Basket> Baskets { get; set; }
    }
}
