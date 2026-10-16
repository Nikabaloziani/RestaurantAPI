using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Report
{
    public class TopProductDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSoldQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
