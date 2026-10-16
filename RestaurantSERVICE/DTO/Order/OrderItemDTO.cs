using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Order
{
    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
