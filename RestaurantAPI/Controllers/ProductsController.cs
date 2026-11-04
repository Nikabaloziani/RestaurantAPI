using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/restaurants/{restaurantId:guid}/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _products;
        private readonly UserManager<ApplicationUser> _users;

        public ProductsController(IProductService products, UserManager<ApplicationUser> users)
        {
            _products = products;
            _users = users;
        }

        private async Task<IActionResult> HandleAsync(Func<string, Task<IActionResult>> work)
        {
            var userId = _users.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            try { return await work(userId); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet]
        public Task<IActionResult> GetAll(Guid restaurantId, CancellationToken ct) =>
            HandleAsync(async userId => Ok(await _products.GetAllAsync(restaurantId, userId, ct)));

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(Guid restaurantId, int id, CancellationToken ct) =>
            HandleAsync(async userId =>
            {
                var product = await _products.GetByIdAsync(restaurantId, id, userId, ct);
                return product is null ? NotFound() : Ok(product);
            });

        [HttpPost]
        public Task<IActionResult> Create(Guid restaurantId, [FromBody] CreateProductDTO dto, CancellationToken ct) =>
            HandleAsync(async userId =>
            {
                var created = await _products.AddAsync(restaurantId, dto, userId, ct);
                return CreatedAtAction(nameof(GetById), new { restaurantId, id = created.Id }, created);
            });

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(Guid restaurantId, int id, [FromBody] UpdateProductDTO dto, CancellationToken ct) =>
            HandleAsync(async userId =>
            {
                dto.Id = id; // The URL identifies the product, not a client-supplied body id.
                return await _products.UpdateAsync(restaurantId, dto, userId, ct)
                    ? Ok("Product updated successfully.") : NotFound();
            });

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(Guid restaurantId, int id, CancellationToken ct) =>
            HandleAsync(async userId =>
                await _products.DeleteAsync(restaurantId, id, userId, ct)
                    ? Ok("Product deleted successfully.") : NotFound());

        [HttpPost("filter")]
        public Task<IActionResult> Filter(Guid restaurantId, [FromBody] ProductFilterDTO filter, CancellationToken ct) =>
            HandleAsync(async userId => Ok(await _products.GetFilteredAsync(restaurantId, filter, userId, ct)));
    }
}
