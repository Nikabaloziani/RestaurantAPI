using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Basket;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE;

public class BasketService : IBasketService
{
    private readonly RestaurantDB _db;
    private readonly IMapper _mapper;

    public BasketService(RestaurantDB db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private Task EnsureAsync(Guid id, string user, CancellationToken ct) =>
        RestaurantTenantAccess.EnsureAsync(_db, id, user, ct,
            RestaurantRole.Owner, RestaurantRole.Manager, RestaurantRole.Waiter, RestaurantRole.Cashier);

    // No Basket.RestaurantId column is needed: ownership derives from Product.RestaurantId.
    // Legacy baskets with NULL/foreign-tenant products are not included or deleted.
    private IQueryable<Basket> Mine(Guid id, string user) =>
        _db.Baskets.Where(b => b.UserId == user && b.Product != null &&
                               b.Product.RestaurantId == id);

    public async Task<List<BasketDTO>> GetMyBasketAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        var rows = await Mine(restaurantId, userId).AsNoTracking()
            .Include(b => b.Product).OrderBy(b => b.Id).ToListAsync(ct);
        return _mapper.Map<List<BasketDTO>>(rows);
    }

    public async Task<BasketDTO> AddToCartAsync(Guid restaurantId, string userId, CreateBasketDTO dto, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        if (dto.Quantity < 1 || dto.Quantity > 100) throw new ArgumentException("Invalid quantity.");

        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.RestaurantId == restaurantId &&
                                      p.Category.RestaurantId == restaurantId, ct);
        if (product is null) throw new ArgumentException("Product not found in this restaurant.");

        var row = await Mine(restaurantId, userId)
            .Include(b => b.Product)
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId, ct);
        if (row is null)
        {
            row = new Basket
            {
                UserId = userId, ProductId = dto.ProductId,
                Quantity = dto.Quantity, UnitPrice = product.Price,
            };
            _db.Baskets.Add(row);
        }
        else
        {
            if (row.Quantity + dto.Quantity > 100) throw new ArgumentException("Basket quantity cannot exceed 100.");
            row.Quantity += dto.Quantity;
        }
        await _db.SaveChangesAsync(ct);
        var saved = await Mine(restaurantId, userId).AsNoTracking()
            .Include(b => b.Product).SingleAsync(b => b.Id == row.Id, ct);
        return _mapper.Map<BasketDTO>(saved);
    }

    public async Task<bool> UpdateQuantityAsync(Guid restaurantId, string userId, int itemId, int quantity, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        if (quantity < 1 || quantity > 100) throw new ArgumentException("Invalid quantity.");
        var row = await Mine(restaurantId, userId).FirstOrDefaultAsync(b => b.Id == itemId, ct);
        if (row is null) return false;
        row.Quantity = quantity;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveFromCartAsync(Guid restaurantId, string userId, int itemId, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        var row = await Mine(restaurantId, userId).FirstOrDefaultAsync(b => b.Id == itemId, ct);
        if (row is null) return false;
        _db.Baskets.Remove(row);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task ClearCartAsync(Guid restaurantId, string userId, CancellationToken ct = default)
    {
        await EnsureAsync(restaurantId, userId, ct);
        var rows = await Mine(restaurantId, userId).ToListAsync(ct);
        _db.Baskets.RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
    }
}
