using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.DTO.Table;

namespace RestaurantSERVICE.Interfaces;

public interface IReservationService
{
    Task<ServiceResult<List<TableDTO>>> GetAvailableTablesAsync(
        Guid restaurantId, DateTimeOffset startTime, int guestCount,
        int durationMinutes, CancellationToken ct = default);

    Task<ServiceResult<ReservationDTO>> CreateAsync(
        Guid restaurantId, string userId, CreateReservationDTO dto,
        bool isStaff, CancellationToken ct = default);

    Task<List<ReservationDTO>> GetMineAsync(
        Guid restaurantId, string userId, CancellationToken ct = default);

    Task<List<ReservationDTO>> GetAllAsync(
        Guid restaurantId, DateTimeOffset? fromTime,
        DateTimeOffset? toTime, CancellationToken ct = default);

    Task<ServiceResult<ReservationDTO>> GetByIdAsync(
        Guid restaurantId, int id, string userId, bool isStaff,
        CancellationToken ct = default);

    Task<ServiceResult<ReservationDTO>> CancelAsync(
        Guid restaurantId, int id, string userId, bool isStaff,
        CancellationToken ct = default);

    Task<ServiceResult<ReservationDTO>> UpdateAsync(
        Guid restaurantId, int id, string userId, UpdateReservationDTO dto,
        bool isStaff, CancellationToken ct = default);
}
