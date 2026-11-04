using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Category;
using RestaurantSERVICE.Services;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/restaurants/{restaurantId:guid}/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categories;
        private readonly UserManager<ApplicationUser> _users;

        public CategoryController(ICategoryService categories, UserManager<ApplicationUser> users)
        {
            _categories = categories;
            _users = users;
        }


        private async Task<IActionResult> HandleAsync(
            Func<string, Task<IActionResult>> work)
        {
            var userId = _users.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                return await work(userId);
            }
            catch (OperationCanceledException)
                when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                return new EmptyResult();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        [HttpGet]
        public Task<IActionResult> GetAll(Guid restaurantId, CancellationToken ct) =>
            HandleAsync(async userId => Ok(await _categories.GetAllAsync(restaurantId, userId, ct)));

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(Guid restaurantId, int id, CancellationToken ct) =>
            HandleAsync(async userId =>
            {
                var category = await _categories.GetByIdAsync(restaurantId, id, userId, ct);
                return category is null ? NotFound() : Ok(category);
            });

        [HttpPost]
        public Task<IActionResult> Create(Guid restaurantId, [FromBody] CreateCategoryDTO dto, CancellationToken ct) =>
            HandleAsync(async userId =>
            {
                await _categories.AddAsync(restaurantId, dto, userId, ct);
                return Ok("Category added successfully.");
            });

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(Guid restaurantId, int id, [FromBody] UpdateCategoryDTO dto, CancellationToken ct) =>
            HandleAsync(async userId =>
                await _categories.UpdateAsync(restaurantId, id, dto, userId, ct)
                    ? Ok("Category updated successfully.") : NotFound());

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(Guid restaurantId, int id, CancellationToken ct) =>
            HandleAsync(async userId =>
                await _categories.DeleteAsync(restaurantId, id, userId, ct)
                    ? Ok("Category deleted successfully.") : NotFound());
    }
}
