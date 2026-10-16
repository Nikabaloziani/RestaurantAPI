using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDTO dto)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(dto);
                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound("Payment not found.");

            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null) return NotFound("Payment for this order not found.");

            return Ok(payment);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Waiter")]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PaymentStatus status)
        {
            var updated = await _paymentService.UpdatePaymentStatusAsync(id, status);
            if (!updated) return NotFound("Payment not found.");

            return Ok("Payment status updated successfully.");
        }
    }
}