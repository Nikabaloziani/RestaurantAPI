using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE;

public class PaymentService : IPaymentService
{
    private readonly RestaurantDB _db;
    private readonly IMapper _mapper;
    public PaymentService(RestaurantDB db, IMapper mapper) { _db = db; _mapper = mapper; }

    private Task EnsureRead(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager, RestaurantRole.Waiter, RestaurantRole.Cashier);

    private Task EnsureWrite(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager, RestaurantRole.Waiter, RestaurantRole.Cashier);

    // Defense in depth: require tenant IDs on both Payment and parent Order.
    private IQueryable<Payment> InRestaurant(Guid id) =>
        _db.Payments.Where(p => p.RestaurantId == id && p.Order != null && p.Order.RestaurantId == id);

    public async Task<PaymentDTO> ProcessPaymentAsync(Guid restaurantId, string userId, CreatePaymentDTO dto,
        CancellationToken ct = default)
    {
        await EnsureWrite(restaurantId, userId, ct);
        if (dto.Method != PaymentMethod.Cash && dto.Method != PaymentMethod.Card)
            throw new InvalidOperationException("Only manually confirmed cash or card payments are supported.");
        if (dto.Amount <= 0) throw new InvalidOperationException("Amount must be greater than zero.");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        // Lock the tenant's order. Another tenant's order ID is deliberately not found.
        var rows = await _db.Orders.FromSqlInterpolated(
            $"SELECT * FROM \"Orders\" WHERE \"Id\" = {dto.OrderId} AND \"RestaurantId\" = {restaurantId} FOR UPDATE")
            .ToListAsync(ct);
        var order = rows.SingleOrDefault();
        if (order is null) throw new ArgumentException("Order not found in this restaurant.");
        if (order.Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cancelled orders cannot be paid.");
        if (order.Status == OrderStatus.Paid) throw new InvalidOperationException("Order is already paid.");
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Preparing &&
            order.Status != OrderStatus.Ready && order.Status != OrderStatus.Served)
            throw new InvalidOperationException("Unsupported order status for payment.");
        if (dto.Amount != order.TotalAmount) throw new InvalidOperationException("Amount must match the order total.");
        if (await _db.Payments.AnyAsync(p => p.OrderId == order.Id && p.Status == PaymentStatus.Completed, ct))
            throw new InvalidOperationException("A completed payment already exists for this order.");

        var payment = new Payment
        {
            RestaurantId = restaurantId,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Method = dto.Method,
            Status = PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            TransactionId = null
        };
        _db.Payments.Add(payment);
        order.Status = OrderStatus.Paid;
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return _mapper.Map<PaymentDTO>(payment);
    }

    public async Task<PaymentDTO?> GetPaymentByIdAsync(Guid restaurantId, string userId, int id, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var payment = await InRestaurant(restaurantId).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        return payment is null ? null : _mapper.Map<PaymentDTO>(payment);
    }

    public async Task<PaymentDTO?> GetPaymentByOrderIdAsync(Guid restaurantId, string userId, int orderId, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var payment = await InRestaurant(restaurantId).AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.PaymentDate).FirstOrDefaultAsync(ct);
        return payment is null ? null : _mapper.Map<PaymentDTO>(payment);
    }

    public async Task<List<PaymentDTO>> GetAllPaymentsAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var payments = await InRestaurant(restaurantId).AsNoTracking()
            .OrderByDescending(p => p.PaymentDate).ToListAsync(ct);
        return _mapper.Map<List<PaymentDTO>>(payments);
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid restaurantId, string userId, int id,
        PaymentStatus status, CancellationToken ct = default)
    {
        await RestaurantTenantAccess.EnsureAsync(_db, restaurantId, userId, ct,
            RestaurantRole.Owner, RestaurantRole.Manager);
        var existing = await InRestaurant(restaurantId).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing is null) return false;
        if (existing.Status == status) return true;
        throw new InvalidOperationException("Payment status cannot be changed manually. Refunds are not implemented.");
    }
}
