using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Report;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class ReportService : IReportService
    {
        private readonly RestaurantDB _context;

        public ReportService(RestaurantDB context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
        {
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totalOrders = await _context.Orders.CountAsync();
            var activeOrders = await _context.Orders
                .CountAsync(o => o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

            var totalReservations = await _context.Reservations.CountAsync();

            return new DashboardSummaryDTO
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                ActiveOrders = activeOrders,
                TotalReservations = totalReservations
            };
        }

        public async Task<List<TopProductDTO>> GetTopSellingProductsAsync(int count = 5)
        {
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, ProductName = oi.Product != null ? oi.Product.Name : "Unknown" })
                .Select(g => new TopProductDTO
                {
                    ProductName = g.Key.ProductName,
                    TotalSoldQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.TotalSoldQuantity)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        public async Task<decimal> GetRevenueByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var revenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return revenue;
        }
    }
}