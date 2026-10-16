using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found.");

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            try
            {
                var result = await _productService.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductDTO dto)
        {
            try
            {
                var updated = await _productService.UpdateAsync(dto);
                if (!updated) return NotFound("Product not found.");

                return Ok("Product updated successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted) return NotFound("Product not found.");

            return Ok("Product deleted successfully.");
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] ProductFilterDTO filter)
        {
            var result = await _productService.GetFilteredAsync(filter);
            return Ok(result);
        }
    }
}