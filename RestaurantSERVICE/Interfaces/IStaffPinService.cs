using RestaurantSERVICE.DTO.StaffPin;

namespace RestaurantSERVICE.Interfaces;

public interface IStaffPinService
{
    Task<MyPinStatusDTO> GetMyStatusAsync(Guid restaurantId, string userId, CancellationToken ct);
    Task<MyPinStatusDTO> SetMyPinAsync(Guid restaurantId, string userId, SetMyPinDTO dto, CancellationToken ct);
    Task DisableMyPinAsync(Guid restaurantId, string userId, DisableMyPinDTO dto, CancellationToken ct);
}
