using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.DTO.Order;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants/{restaurantId:guid}/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private async Task<IActionResult> Handle(Func<Task<IActionResult>> action)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
        try { return await action(); }
        // A browser AbortController may cancel requests during a restaurant switch.
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public Task<IActionResult> CreateOrder(Guid restaurantId, [FromBody] CreateOrderDTO dto, CancellationToken ct) =>
        Handle(async () => Ok(await _orders.CreateOrderFromBasketAsync(restaurantId, UserId, dto, ct)));

    [HttpGet("my")]
    public Task<IActionResult> GetMyOrders(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => Ok(await _orders.GetMyOrdersAsync(restaurantId, UserId, ct)));

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(Guid restaurantId, int id, CancellationToken ct) =>
        Handle(async () => {
            var value = await _orders.GetOrderByIdAsync(restaurantId, id, UserId, ct);
            return value is null ? NotFound() : Ok(value);
        });

    [HttpGet]
    public Task<IActionResult> GetAll(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => Ok(await _orders.GetAllOrdersAsync(restaurantId, UserId, ct)));

    [HttpPatch("{id:int}/status")]
    public Task<IActionResult> UpdateStatus(Guid restaurantId, int id, [FromBody] UpdateOrderStatusDTO dto, CancellationToken ct) =>
        Handle(async () => await _orders.UpdateStatusAsync(restaurantId, id, dto.Status, UserId, ct)
            ? Ok("Order status updated.") : NotFound());
}
