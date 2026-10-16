using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.DTO.Basket;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetMyBasket()
        {
            var basket = await _basketService.GetMyBasketAsync(GetUserId());
            return Ok(basket);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CreateBasketDTO dto)
        {
            var result = await _basketService.AddToCartAsync(GetUserId(), dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateBasketDTO dto)
        {
            var updated = await _basketService.UpdateQuantityAsync(GetUserId(), id, dto.Quantity);
            if (!updated) return NotFound("Basket item not found.");

            return Ok("Basket updated.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var removed = await _basketService.RemoveFromCartAsync(GetUserId(), id);
            if (!removed) return NotFound("Basket item not found.");

            return Ok("Item removed from basket.");
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _basketService.ClearCartAsync(GetUserId());
            return Ok("Basket cleared.");
        }
    }
}