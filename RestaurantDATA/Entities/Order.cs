using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int TableId { get; set; }
        public Table? Table { get; set; }
        public Guid? RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
