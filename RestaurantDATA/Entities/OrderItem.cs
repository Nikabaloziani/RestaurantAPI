using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Products? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
