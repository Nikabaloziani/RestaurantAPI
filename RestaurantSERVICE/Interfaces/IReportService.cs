using RestaurantSERVICE.DTO.Report;

namespace RestaurantSERVICE.Interfaces;

public interface IReportService
{
    Task<DashboardSummaryDTO> GetDashboardSummaryAsync(Guid restaurantId, string userId, CancellationToken ct = default);
    Task<List<TopProductDTO>> GetTopSellingProductsAsync(Guid restaurantId, string userId, int count = 5, CancellationToken ct = default);
    Task<decimal> GetRevenueByDateRangeAsync(Guid restaurantId, string userId,
        DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default);
}
