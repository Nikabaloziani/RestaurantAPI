using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Restaurant;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RestaurantController(
            IRestaurantService restaurantService,
            UserManager<ApplicationUser> userManager)
        {
            _restaurantService = restaurantService;
            _userManager = userManager;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateRestaurantDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            try
            {
                var restaurant = await _restaurantService.CreateAsync(
                    dto, userId, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, restaurant);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException) { return NotFound("Organization not found."); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (DbUpdateException ex) when
                (ex.InnerException is PostgresException pg &&
                 pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict("Restaurant slug or code already exists.");
            }
            catch (DbUpdateException ex) when
                (ex.InnerException is PostgresException pg &&
                 pg.SqlState == PostgresErrorCodes.SerializationFailure)
            {
                return Conflict("Concurrent change detected. Retry your request.");
            }
            catch (PostgresException ex) when
                (ex.SqlState == PostgresErrorCodes.SerializationFailure)
            {
                return Conflict("Concurrent change detected. Retry your request.");
            }
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var restaurants = await _restaurantService.GetMineAsync(
                userId, cancellationToken);
            return Ok(restaurants);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var restaurant = await _restaurantService.GetByIdAsync(
                id, userId, cancellationToken);
            return restaurant == null ? NotFound() : Ok(restaurant);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRestaurantDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var restaurant = await _restaurantService.UpdateAsync(
                id, dto, userId, cancellationToken);
            return Ok(restaurant);
        }
    }
}
