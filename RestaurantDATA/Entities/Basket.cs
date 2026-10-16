using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantDATA.Entities
{
    [Table("Baskets")]
    public class Basket
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Products? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}