using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants/{restaurantId:guid}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    public ReportsController(IReportService reports) => _reports = reports;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private async Task<IActionResult> Handle(Func<Task<IActionResult>> action)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
        try { return await action(); }
        // A browser AbortController may cancel requests during a restaurant switch.
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("summary")]
    public Task<IActionResult> Summary(Guid restaurantId, CancellationToken ct) =>
        Handle(async () => Ok(await _reports.GetDashboardSummaryAsync(restaurantId, UserId, ct)));

    [HttpGet("top-products")]
    public Task<IActionResult> TopProducts(Guid restaurantId, [FromQuery] int count = 5, CancellationToken ct = default) =>
        Handle(async () => Ok(await _reports.GetTopSellingProductsAsync(restaurantId, UserId, count, ct)));

    [HttpGet("revenue")]
    public Task<IActionResult> Revenue(Guid restaurantId, [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset endDate, CancellationToken ct) =>
        Handle(async () => Ok(new
        {
            startDate, endDate,
            totalRevenue = await _reports.GetRevenueByDateRangeAsync(restaurantId, UserId, startDate, endDate, ct)
        }));
}
