using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants/{restaurantId:guid}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;
    public PaymentsController(IPaymentService payments) => _payments = payments;
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
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    // Records already-collected cash / external terminal payments; NOT an online charge.
    [HttpPost]
    public Task<IActionResult> ProcessPayment(Guid restaurantId, [FromBody] CreatePaymentDTO dto, CancellationToken ct) =>
        Handle(async () => Ok(await _payments.ProcessPaymentAsync(restaurantId, UserId, dto, ct)));

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(Guid restaurantId, int id, CancellationToken ct) =>
        Handle(async () => {
            var payment = await _payments.GetPaymentByIdAsync(restaurantId, UserId, id, ct);
            return payment is null ? NotFound() : Ok(payment);
        });

    [HttpGet("order/{orderId:int}")]
    public Task<IActionResult> GetByOrderId(Guid restaurantId, int orderId, CancellationToken ct) =>
        Handle(async () => {
            var payment = await _payments.GetPaymentByOrderIdAsync(restaurantId, UserId, orderId, ct);
            return payment is null ? NotFound() : Ok(payment);
        });

    [HttpGet]
    public Task<IActionResult> GetAll(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => Ok(await _payments.GetAllPaymentsAsync(restaurantId, UserId, ct)));

    [HttpPatch("{id:int}/status")]
    public Task<IActionResult> UpdateStatus(Guid restaurantId, int id, [FromBody] PaymentStatus status, CancellationToken ct) =>
        Handle(async () => await _payments.UpdatePaymentStatusAsync(restaurantId, UserId, id, status, ct)
            ? Ok("Payment unchanged.") : NotFound());
}
