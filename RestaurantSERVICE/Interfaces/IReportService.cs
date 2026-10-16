using RestaurantSERVICE.DTO.Report;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IReportService
    {
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync();
        Task<List<TopProductDTO>> GetTopSellingProductsAsync(int count = 5);
        Task<decimal> GetRevenueByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
