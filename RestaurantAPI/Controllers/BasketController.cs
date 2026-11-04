using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.DTO.Basket;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants/{restaurantId:guid}/basket")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basket;
    public BasketController(IBasketService basket) => _basket = basket;
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
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet]
    public Task<IActionResult> GetMyBasket(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => Ok(await _basket.GetMyBasketAsync(restaurantId, UserId, ct)));

    [HttpPost]
    public Task<IActionResult> AddToCart(Guid restaurantId, [FromBody] CreateBasketDTO dto, CancellationToken ct) =>
        Handle(async () => Ok(await _basket.AddToCartAsync(restaurantId, UserId, dto, ct)));

    [HttpPut("{id:int}")]
    public Task<IActionResult> UpdateQuantity(Guid restaurantId, int id, [FromBody] UpdateBasketDTO dto, CancellationToken ct) =>
        Handle(async () => await _basket.UpdateQuantityAsync(restaurantId, UserId, id, dto.Quantity, ct)
            ? Ok("Basket updated.") : NotFound());

    [HttpDelete("{id:int}")]
    public Task<IActionResult> RemoveFromCart(Guid restaurantId, int id, CancellationToken ct) =>
        Handle(async () => await _basket.RemoveFromCartAsync(restaurantId, UserId, id, ct)
            ? Ok("Item removed.") : NotFound());

    [HttpDelete("clear")]
    public Task<IActionResult> ClearCart(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => { await _basket.ClearCartAsync(restaurantId, UserId, ct); return Ok("Basket cleared."); });
}
