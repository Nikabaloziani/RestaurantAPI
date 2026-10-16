using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Order
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TableId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }
}
