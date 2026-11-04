using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Report;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE;

public class ReportService : IReportService
{
    private readonly RestaurantDB _db;
    public ReportService(RestaurantDB db) => _db = db;

    private Task EnsureAsync(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager);

    // Both payment and parent order must belong to same restaurant.
    private IQueryable<Payment> Completed(Guid id) =>
        _db.Payments.AsNoTracking().Where(p =>
            p.RestaurantId == id && p.Order != null &&
            p.Order.RestaurantId == id && p.Status == PaymentStatus.Completed);

    public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        var revenue = await Completed(restaurantId).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var orders = _db.Orders.AsNoTracking().Where(o => o.RestaurantId == restaurantId);
        var count = await orders.CountAsync(ct);
        var active = await orders.CountAsync(o =>
            o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled, ct);
        var reservations = await _db.Reservations.AsNoTracking()
            .CountAsync(r => r.RestaurantId == restaurantId, ct);

        return new DashboardSummaryDTO
        {
            TotalRevenue = revenue,
            TotalOrders = count,
            ActiveOrders = active,
            TotalReservations = reservations
        };
    }

    public async Task<List<TopProductDTO>> GetTopSellingProductsAsync(Guid restaurantId, string userId,
        int count = 5, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        count = Math.Clamp(count, 1, 50);
        return await _db.OrderItems.AsNoTracking()
            .Where(oi => oi.RestaurantId == restaurantId && oi.Order != null &&
                oi.Order.RestaurantId == restaurantId && oi.Product != null && oi.Product.RestaurantId == restaurantId && oi.Order.Status == OrderStatus.Paid &&
                _db.Payments.Any(p => p.RestaurantId == restaurantId &&
                    p.OrderId == oi.OrderId && p.Status == PaymentStatus.Completed))
            .GroupBy(oi => new
            {
                oi.ProductId,
                ProductName = oi.Product != null ? oi.Product.Name : "Unknown"
            })
            .Select(g => new TopProductDTO
            {
                ProductName = g.Key.ProductName,
                TotalSoldQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.TotalSoldQuantity)
            .Take(count).ToListAsync(ct);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(Guid restaurantId, string userId,
        DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        if (endDate < startDate) throw new ArgumentException("endDate must be after startDate.");
        return await Completed(restaurantId)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
    }
}
