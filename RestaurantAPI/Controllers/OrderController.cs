using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Order;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            try
            {
                var order = await _orderService.CreateOrderFromBasketAsync(GetUserId(), dto);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _orderService.GetMyOrdersAsync(GetUserId());
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found.");
            if (!User.IsInRole("Admin") && !User.IsInRole("Waiter") && order.UserId != GetUserId())
                return Forbid();

            return Ok(order);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Waiter")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Waiter")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            var updated = await _orderService.UpdateStatusAsync(id, dto.Status);
            if (!updated) return NotFound("Order not found.");

            return Ok("Order status updated successfully.");
        }
    }
}