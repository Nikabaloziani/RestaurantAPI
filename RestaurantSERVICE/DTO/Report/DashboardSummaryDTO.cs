using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Report
{
    public class DashboardSummaryDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalReservations { get; set; }
        public int ActiveOrders { get; set; }
    }
}
