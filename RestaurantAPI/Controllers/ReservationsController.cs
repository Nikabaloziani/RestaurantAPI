using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
// The old /api/Reservations route must NOT remain active: it was global.
[Route("api/restaurants/{restaurantId:guid}/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly RestaurantDB _db;

    public ReservationsController(IReservationService service, RestaurantDB db)
    {
        _service = service;
        _db = db;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Restaurant membership, NOT ASP.NET Identity's global Admin/Waiter roles.
    private Task<bool> IsReservationStaffAsync(Guid restaurantId, string userId, CancellationToken ct) =>
        _db.RestaurantUsers.AsNoTracking().AnyAsync(ru =>
            ru.RestaurantId == restaurantId && ru.UserId == userId &&
            ru.Restaurant.IsActive &&
            (ru.Role == RestaurantRole.Owner ||
             ru.Role == RestaurantRole.Manager ||
             ru.Role == RestaurantRole.Waiter), ct);

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableTables(
        Guid restaurantId,
        [FromQuery] DateTimeOffset? startTime,
        [FromQuery] int guestCount = 2,
        [FromQuery] int durationMinutes = 120,
        CancellationToken ct = default)
    {
        if (startTime is null)
            return BadRequest(new { message = "startTime is required." });

        var result = await _service.GetAvailableTablesAsync(
            restaurantId, startTime.Value, guestCount, durationMinutes, ct);
        return result.IsSuccess ? Ok(result.Data) : Error(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid restaurantId,
        [FromQuery] DateTimeOffset? fromTime,
        [FromQuery] DateTimeOffset? toTime, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!await IsReservationStaffAsync(restaurantId, userId, ct)) return Forbid();
        var items = await _service.GetAllAsync(restaurantId, fromTime, toTime, ct);
        return Ok(items);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine(Guid restaurantId, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(await _service.GetMineAsync(restaurantId, userId, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(Guid restaurantId, int id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var isStaff = await IsReservationStaffAsync(restaurantId, userId, ct);
        var result = await _service.GetByIdAsync(restaurantId, id, userId, isStaff, ct);
        return result.IsSuccess ? Ok(result.Data) : Error(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid restaurantId,
        [FromBody] CreateReservationDTO dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var isStaff = await IsReservationStaffAsync(restaurantId, userId, ct);
        var result = await _service.CreateAsync(restaurantId, userId, dto, isStaff, ct);
        if (!result.IsSuccess) return Error(result);
        return CreatedAtAction(nameof(GetById),
            new { restaurantId, id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(Guid restaurantId, int id,
        [FromBody] UpdateReservationDTO dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var isStaff = await IsReservationStaffAsync(restaurantId, userId, ct);
        var result = await _service.UpdateAsync(restaurantId, id, userId, dto, isStaff, ct);
        return result.IsSuccess ? Ok(result.Data) : Error(result);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(Guid restaurantId, int id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var isStaff = await IsReservationStaffAsync(restaurantId, userId, ct);
        var result = await _service.CancelAsync(restaurantId, id, userId, isStaff, ct);
        return result.IsSuccess ? Ok(result.Data) : Error(result);
    }

    private IActionResult Error<T>(ServiceResult<T> result)
    {
        var body = new { message = result.Error };
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(body),
            ServiceErrorType.Conflict => Conflict(body),
            ServiceErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
            _ => BadRequest(body)
        };
    }
}
