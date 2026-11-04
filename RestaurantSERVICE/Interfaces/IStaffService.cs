using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Staff;

namespace RestaurantSERVICE.Interfaces;

public interface IStaffService
{
    Task<StaffOverviewDTO> GetOverviewAsync(Guid restaurantId, string actorUserId, CancellationToken ct);
    Task<List<StaffInvitationDTO>> GetPendingInvitationsAsync(Guid restaurantId, string actorUserId, CancellationToken ct);
    Task<CreatedStaffInvitationDTO> InviteAsync(Guid restaurantId, string actorUserId, InviteStaffDTO dto, CancellationToken ct);
    Task<StaffMemberDTO> AcceptAsync(string actorUserId, string token, CancellationToken ct);
    Task<StaffMemberDTO> ChangeRoleAsync(Guid restaurantId, string memberUserId, string actorUserId, RestaurantRole role, CancellationToken ct);
    Task RemoveAsync(Guid restaurantId, string memberUserId, string actorUserId, CancellationToken ct);
    Task RevokeInvitationAsync(Guid restaurantId, Guid invitationId, string actorUserId, CancellationToken ct);
}
