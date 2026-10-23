using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Constants;
using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.Interfaces;
using System.Security.Claims;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        private string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private bool IsStaff => User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Waiter);


        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTables(
            [FromQuery] DateTimeOffset? startTime,
            [FromQuery] int guestCount = 2,
            [FromQuery] int durationMinutes = 120)
        {
            if (startTime == null)
            {
                return BadRequest(new { message = "startTime is required (ISO 8601, e.g. 2026-10-10T19:00:00Z)." });
            }

            var result = await _reservationService.GetAvailableTablesAsync(startTime.Value, guestCount, durationMinutes);
            if (!result.IsSuccess)
            {
                return Error(result);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDTO dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _reservationService.CreateAsync(userId, dto, IsStaff);
            if (!result.IsSuccess)
            {
                return Error(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reservations = await _reservationService.GetMineAsync(userId);
            return Ok(reservations);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Staff)]
        public async Task<IActionResult> GetAll([FromQuery] DateTimeOffset? fromTime, [FromQuery] DateTimeOffset? toTime)
        {
            var reservations = await _reservationService.GetAllAsync(fromTime, toTime);
            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _reservationService.GetByIdAsync(id, userId, IsStaff);
            if (!result.IsSuccess)
            {
                return Error(result);
            }

            return Ok(result.Data);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _reservationService.CancelAsync(id, userId, IsStaff);
            if (!result.IsSuccess)
            {
                return Error(result);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDTO dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _reservationService.UpdateAsync(id, userId, dto, IsStaff);
            if (!result.IsSuccess)
            {
                return Error(result);
            }

            return Ok(result.Data);
        }

        private IActionResult Error<T>(ServiceResult<T> result)
        {
            var body = new { message = result.Error };

            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound(body);
            }

            if (result.ErrorType == ServiceErrorType.Conflict)
            {
                return Conflict(body);
            }

            if (result.ErrorType == ServiceErrorType.Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, body);
            }

            return BadRequest(body);
        }
    }
}
