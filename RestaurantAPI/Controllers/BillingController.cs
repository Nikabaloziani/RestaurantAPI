using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Billing;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly IOrganizationBillingService _billing;
    private readonly UserManager<ApplicationUser> _users;

    public BillingController(IOrganizationBillingService billing,
                             UserManager<ApplicationUser> users)
    {
        _billing = billing;
        _users = users;
    }

    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans(CancellationToken ct) =>
        Ok(await _billing.GetPlansAsync(ct));

    [Authorize]
    [HttpPost("organizations")]
    public async Task<IActionResult> CreateOrganization(
        [FromBody] CreateOrganizationDTO dto, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        try
        {
            var org = await _billing.CreateOrganizationAsync(userId, dto.Name, ct);
            return StatusCode(StatusCodes.Status201Created, org);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (DbUpdateException) { return Conflict("An organization already exists for this user."); }
    }

    [Authorize]
    [HttpGet("organizations/mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        return string.IsNullOrWhiteSpace(userId) ? Unauthorized() :
            Ok(await _billing.GetMineAsync(userId, ct));
    }

    [Authorize]
    [HttpGet("organizations/{organizationId:guid}/subscription")]
    public async Task<IActionResult> GetSubscription(Guid organizationId, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var subscription = await _billing.GetSubscriptionAsync(organizationId, userId, ct);
        return subscription == null ? NotFound() : Ok(subscription);
    }

    [Authorize]
    [HttpPost("organizations/{organizationId:guid}/restaurants/{restaurantId:guid}/attach")]
    public async Task<IActionResult> AttachRestaurant(
        Guid organizationId, Guid restaurantId, CancellationToken ct)
    {
        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        try
        {
            var updated = await _billing.AttachExistingRestaurantAsync(
                organizationId, restaurantId, userId, ct);
            return Ok(updated);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        // A PostgreSQL serialization failure returns an error rather than
        // bypassing a location limit. Client can retry safely.
    }
}
