using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.StaffPin;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants/{restaurantId:guid}/staff-pin")]
public sealed class StaffPinController : ControllerBase
{
    private readonly IStaffPinService _pins;
    private readonly UserManager<ApplicationUser> _users;

    public StaffPinController(IStaffPinService pins, UserManager<ApplicationUser> users)
    {
        _pins = pins;
        _users = users;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyStatus(Guid restaurantId, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try { return Ok(await _pins.GetMyStatusAsync(restaurantId, userId, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("me")]
    [EnableRateLimiting("staff-pin-management")]
    [ProducesResponseType(typeof(MyPinStatusDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetMyPin(
        Guid restaurantId, [FromBody] SetMyPinDTO dto, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try { return Ok(await _pins.SetMyPinAsync(restaurantId, userId, dto, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgres &&
                                           postgres.SqlState == PostgresErrorCodes.UniqueViolation)
        { return Conflict(new { message = "PIN was updated concurrently. Retry." }); }
    }

    [HttpPost("me/disable")]
    [EnableRateLimiting("staff-pin-management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DisableMyPin(
        Guid restaurantId, [FromBody] DisableMyPinDTO dto, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try
        {
            await _pins.DisableMyPinAsync(restaurantId, userId, dto, ct);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
