
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Create(
            [FromBody] CreateRestaurantDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var restaurant = await _restaurantService.CreateAsync(
                dto,
                userId,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                restaurant);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var restaurants = await _restaurantService.GetMineAsync(
                userId,
                cancellationToken);

            return Ok(restaurants);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var restaurant = await _restaurantService.GetByIdAsync(
                id,
                userId,
                cancellationToken);

            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRestaurantDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var restaurant = await _restaurantService.UpdateAsync(
                id,
                dto,
                userId,
                cancellationToken);

            return Ok(restaurant);
        }
    }
}
