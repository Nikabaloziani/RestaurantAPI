using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.DTO.Table;

namespace RestaurantSERVICE.Interfaces
{
    public interface IReservationService
    {
        Task<ServiceResult<List<TableDTO>>> GetAvailableTablesAsync(DateTimeOffset startTime, int guestCount, int durationMinutes);

        Task<ServiceResult<ReservationDTO>> CreateAsync(string userId, CreateReservationDTO dto);

        Task<List<ReservationDTO>> GetMineAsync(string userId);

        Task<List<ReservationDTO>> GetAllAsync(DateTimeOffset? fromTime, DateTimeOffset? toTime);

        Task<ServiceResult<ReservationDTO>> GetByIdAsync(int id, string userId, bool isStaff);

        Task<ServiceResult<ReservationDTO>> CancelAsync(int id, string userId, bool isStaff);

        Task<ServiceResult<ReservationDTO>> UpdateAsync(int id, string userId, UpdateReservationDTO dto, bool isStaff);
    }
}
