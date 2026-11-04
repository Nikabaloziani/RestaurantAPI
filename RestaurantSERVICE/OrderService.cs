using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Order;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE;

public class OrderService : IOrderService
{
    private readonly RestaurantDB _db;
    private readonly IMapper _mapper;

    public OrderService(RestaurantDB db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private Task EnsureRead(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager, RestaurantRole.Waiter, RestaurantRole.Cashier);

    private Task EnsureWrite(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager, RestaurantRole.Waiter);

    private IQueryable<Order> InRestaurant(Guid id) =>
        _db.Orders.Where(o => o.RestaurantId == id);

    public async Task<OrderDTO> CreateOrderFromBasketAsync(Guid restaurantId, string userId,
        CreateOrderDTO dto, CancellationToken ct = default)
    {
        await EnsureWrite(restaurantId, userId, ct);
        if (dto.TableId <= 0) throw new InvalidOperationException("Select a table.");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        bool tableValid = await _db.Tables.AsNoTracking().AnyAsync(t =>
            t.Id == dto.TableId && t.RestaurantId == restaurantId &&
            t.IsActive && !t.IsDeleted, ct);
        if (!tableValid) throw new InvalidOperationException("Table not found in this restaurant.");

        var rows = await _db.Baskets.Include(b => b.Product)
            .Where(b => b.UserId == userId && b.Product != null &&
                        b.Product.RestaurantId == restaurantId &&
                        b.Product.Category.RestaurantId == restaurantId)
            .OrderBy(b => b.Id).ToListAsync(ct);

        if (rows.Count == 0) throw new InvalidOperationException("Basket is empty for this restaurant.");
        if (rows.Any(b => b.Quantity < 1)) throw new InvalidOperationException("Invalid basket quantity.");

        var order = new Order
        {
            RestaurantId = restaurantId,
            UserId = userId,
            TableId = dto.TableId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = rows.Sum(b => b.UnitPrice * b.Quantity),
            OrderItems = rows.Select(b => new OrderItem
            {
                RestaurantId = restaurantId,
                ProductId = b.ProductId,
                Quantity = b.Quantity,
                UnitPrice = b.UnitPrice
            }).ToList()
        };

        _db.Orders.Add(order);
        _db.Baskets.RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var saved = await InRestaurant(restaurantId).AsNoTracking()
            .Include(o => o.OrderItems.Where(i => i.RestaurantId == restaurantId && i.Product != null && i.Product.RestaurantId == restaurantId)).ThenInclude(i => i.Product)
            .Include(o => o.Table)
            .SingleAsync(o => o.Id == order.Id, ct);
        return _mapper.Map<OrderDTO>(saved);
    }

    public async Task<OrderDTO?> GetOrderByIdAsync(Guid restaurantId, int id, string userId, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var order = await InRestaurant(restaurantId).AsNoTracking()
            .Include(o => o.OrderItems.Where(i => i.RestaurantId == restaurantId && i.Product != null && i.Product.RestaurantId == restaurantId)).ThenInclude(i => i.Product)
            .Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? null : _mapper.Map<OrderDTO>(order);
    }

    public async Task<List<OrderDTO>> GetMyOrdersAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var orders = await InRestaurant(restaurantId).AsNoTracking()
            .Include(o => o.OrderItems.Where(i => i.RestaurantId == restaurantId && i.Product != null && i.Product.RestaurantId == restaurantId)).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate).ToListAsync(ct);
        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<List<OrderDTO>> GetAllOrdersAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureRead(restaurantId, userId, ct);
        var orders = await InRestaurant(restaurantId).AsNoTracking()
            .Include(o => o.OrderItems.Where(i => i.RestaurantId == restaurantId && i.Product != null && i.Product.RestaurantId == restaurantId)).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate).ToListAsync(ct);
        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<bool> UpdateStatusAsync(Guid restaurantId, int id, OrderStatus status,
        string userId, CancellationToken ct = default)
    {
        await EnsureWrite(restaurantId, userId, ct);
        if (!Enum.IsDefined(typeof(OrderStatus), status))
            throw new InvalidOperationException("Invalid order status.");
        if (status == OrderStatus.Paid)
            throw new InvalidOperationException("Paid status must be set by a completed payment.");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orders = await _db.Orders.FromSqlInterpolated(
            $"SELECT * FROM \"Orders\" WHERE \"Id\" = {id} AND \"RestaurantId\" = {restaurantId} FOR UPDATE")
            .ToListAsync(ct);
        var order = orders.SingleOrDefault();
        if (order is null) return false;
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Paid or cancelled orders cannot be changed.");
        order.Status = status;
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}
