using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Basket
{
    public class BasketDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
