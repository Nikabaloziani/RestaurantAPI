using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public Guid? RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Products> Products { get; set; } = new List<Products>();
    }
}
