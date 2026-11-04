
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/restaurants/{restaurantId:guid}/tables")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TablesController(
            ITableService tableService,
            UserManager<ApplicationUser> userManager)
        {
            _tableService = tableService;
            _userManager = userManager;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllTables(
    Guid restaurantId,
    CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var tables = await _tableService.GetAllTablesAsync(
                    restaurantId,
                    userId,
                    cancellationToken);

                return Ok(tables);
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
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            Guid restaurantId,
            int id,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var table = await _tableService.GetByIdAsync(
                    restaurantId,
                    id,
                    userId,
                    cancellationToken);

                if (table == null)
                    return NotFound();

                return Ok(table);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Guid restaurantId,
            [FromBody] CreateTableDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _tableService.AddAsync(
                    restaurantId,
                    dto,
                    userId,
                    cancellationToken);

                return StatusCode(
                    StatusCodes.Status201Created,
                    new { message = "Table created successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(
            Guid restaurantId,
            int id,
            [FromBody] EditTableDTO dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var updated = await _tableService.UpdateDetailsAsync(
                    restaurantId,
                    id,
                    dto,
                    userId,
                    cancellationToken);

                if (!updated)
                    return NotFound();

                return Ok(new
                {
                    message = "Table updated successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateTableStatus(
            Guid restaurantId,
            int id,
            [FromBody] UpdateTableStatusDto dto,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var updated = await _tableService.UpdateStatusAsync(
                    restaurantId,
                    id,
                    dto.IsOccupied,
                    userId,
                    cancellationToken);

                if (!updated)
                    return NotFound();

                return Ok(new
                {
                    message = "Table status updated successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            Guid restaurantId,
            int id,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                var deleted = await _tableService.DeleteAsync(
                    restaurantId,
                    id,
                    userId,
                    cancellationToken);

                if (!deleted)
                    return NotFound();

                return Ok(new
                {
                    message = "Table deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
