using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _reportService.GetDashboardSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5)
        {
            var topProducts = await _reportService.GetTopSellingProductsAsync(count);
            return Ok(topProducts);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate)
        {
            var revenue = await _reportService.GetRevenueByDateRangeAsync(startDate, endDate);
            return Ok(new { startDate, endDate, totalRevenue = revenue });
        }
    }
}