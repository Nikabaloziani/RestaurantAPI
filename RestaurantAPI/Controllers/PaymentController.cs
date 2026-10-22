using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Authorize(Roles = "Admin,Waiter")]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Records money that has ALREADY been collected in cash or on an external
        // card terminal. This endpoint does NOT charge a bank card.
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDTO dto)
        {
            try
            {
                return Ok(await _paymentService.ProcessPaymentAsync(dto));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return payment == null ? NotFound("Payment not found.") : Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            return payment == null ? NotFound("Payment for this order not found.") : Ok(payment);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _paymentService.GetAllPaymentsAsync());
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PaymentStatus status)
        {
            try
            {
                var updated = await _paymentService.UpdatePaymentStatusAsync(id, status);
                return updated ? Ok("Payment unchanged.") : NotFound("Payment not found.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
