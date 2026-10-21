using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TablesController(ITableService tableService)
        {
            _tableService = tableService;
        }


        [HttpGet]
        [Authorize(Roles = "Admin, Waiter")]
        public async Task<IActionResult> GetAllTables()
        {
            var tables = await _tableService.GetAllTablesAsync();
            return Ok(tables);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null) return NotFound("Table not found");
            return Ok(table);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]

        //(Roles = "Admin, admin, Waiter, waiter")
        public async Task<IActionResult> Create([FromBody] CreateTableDTO dto)
        {
            await _tableService.AddAsync(dto);
            return Ok("Table added successfully");
        }


        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, Waiter")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] UpdateTableStatusDto dto)
        {
            var result = await _tableService.UpdateStatusAsync(id, dto.IsOccupied);

            if (!result)
            {
                return NotFound(new { message = $"Table With that ID {id} not found" });
            }

            return Ok(new { message = "Table status updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _tableService.DeleteAsync(id);
            return Ok("Table deleted successfully");
        }
    }
}
