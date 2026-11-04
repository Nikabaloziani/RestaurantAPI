using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestaurantSERVICE.DTO.Staff;
using RestaurantSERVICE.Interfaces;

namespace RestaurantAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/staff")]
public sealed class StaffController : ControllerBase
{
    private readonly IStaffService _staff;
    public StaffController(IStaffService staff) => _staff = staff;
    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("restaurants/{restaurantId:guid}")]
    public Task<IActionResult> GetMembers(Guid restaurantId, CancellationToken ct) =>
        ExecuteAsync(async userId => Ok(await _staff.GetOverviewAsync(restaurantId, userId, ct)));

    [HttpGet("restaurants/{restaurantId:guid}/invitations")]
    public Task<IActionResult> GetInvitations(Guid restaurantId, CancellationToken ct) =>
        ExecuteAsync(async userId => Ok(await _staff.GetPendingInvitationsAsync(restaurantId, userId, ct)));

    [HttpPost("restaurants/{restaurantId:guid}/invitations")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Invite(Guid restaurantId, [FromBody] InviteStaffDTO dto,
        CancellationToken ct) => ExecuteAsync(async userId =>
            StatusCode(StatusCodes.Status201Created,
                await _staff.InviteAsync(restaurantId, userId, dto, ct)));

    [HttpPost("invitations/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Accept([FromBody] AcceptStaffInvitationDTO dto,
        CancellationToken ct) => ExecuteAsync(async userId =>
            Ok(await _staff.AcceptAsync(userId, dto.Token, ct)));

    [HttpPatch("restaurants/{restaurantId:guid}/members/{memberUserId}/role")]
    public Task<IActionResult> ChangeRole(Guid restaurantId, string memberUserId,
        [FromBody] UpdateStaffRoleDTO dto, CancellationToken ct) =>
        ExecuteAsync(async userId => Ok(await _staff.ChangeRoleAsync(
            restaurantId, memberUserId, userId, dto.Role, ct)));

    [HttpDelete("restaurants/{restaurantId:guid}/members/{memberUserId}")]
    public Task<IActionResult> Remove(Guid restaurantId, string memberUserId,
        CancellationToken ct) => ExecuteAsync(async userId =>
        {
            await _staff.RemoveAsync(restaurantId, memberUserId, userId, ct);
            return NoContent();
        });

    [HttpDelete("restaurants/{restaurantId:guid}/invitations/{invitationId:guid}")]
    public Task<IActionResult> Revoke(Guid restaurantId, Guid invitationId,
        CancellationToken ct) => ExecuteAsync(async userId =>
        {
            await _staff.RevokeInvitationAsync(restaurantId, invitationId, userId, ct);
            return NoContent();
        });

    private async Task<IActionResult> ExecuteAsync(Func<string, Task<IActionResult>> action)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
        try { return await action(UserId); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            return Conflict("Concurrent staff change detected. Please retry.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                             pg.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            return Conflict("Concurrent staff change detected. Please retry.");
        }
    }
}
