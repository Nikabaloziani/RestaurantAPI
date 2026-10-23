using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;
using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class ReservationService : IReservationService
    {
        private const int MinDurationMinutes = 30;
        private const int MaxDurationMinutes = 360;
        private const int MaxNoteLength = 500;

        private readonly IReservationRepository _reservationRepository;
        private readonly IBaseRepository<Table> _tableRepository;

        public ReservationService(
            IReservationRepository reservationRepository,
            IBaseRepository<Table> tableRepository)
        {
            _reservationRepository = reservationRepository;
            _tableRepository = tableRepository;
        }

        public async Task<ServiceResult<List<TableDTO>>> GetAvailableTablesAsync(
            DateTimeOffset startTime, int guestCount, int durationMinutes)
        {
            var validationError = Validate(startTime, guestCount, durationMinutes);
            if (validationError != null)
            {
                return ServiceResult<List<TableDTO>>.Fail(ServiceErrorType.Validation, validationError);
            }

            var endTime = startTime.AddMinutes(durationMinutes);
            var tables = await _reservationRepository.GetAvailableTablesAsync(startTime, endTime, guestCount);

            return ServiceResult<List<TableDTO>>.Ok(tables.Select(ToTableDto).ToList());
        }

        public async Task<ServiceResult<ReservationDTO>> CreateAsync(string userId, CreateReservationDTO dto, bool isStaff)
        {
            var validationError = Validate(dto.StartTime, dto.GuestCount, dto.DurationMinutes);
            if (validationError != null)
            {
                return Fail(ServiceErrorType.Validation, validationError);
            }

            var note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            if (note != null && note.Length > MaxNoteLength)
            {
                return Fail(ServiceErrorType.Validation, $"Note can be at most {MaxNoteLength} characters.");
            }

            var guestValidation = ValidateGuestDetails(dto.GuestName, dto.GuestPhone, isStaff);
            if (guestValidation != null)
            {
                return Fail(ServiceErrorType.Validation, guestValidation);
            }

            var table = await _tableRepository.GetByIdAsync(dto.TableId);
            if (table == null)
            {
                return Fail(ServiceErrorType.NotFound, "Table not found.");
            }

            if (dto.GuestCount > table.Capacity)
            {
                return Fail(ServiceErrorType.Validation,
                    $"Table {table.TableNumber} seats up to {table.Capacity} guests.");
            }

            var reservation = new Reservation
            {
                TableId = table.Id,
                UserId = userId,
                StartTime = dto.StartTime,
                EndTime = dto.StartTime.AddMinutes(dto.DurationMinutes),
                GuestCount = dto.GuestCount,
                Note = note,
                GuestName = isStaff ? dto.GuestName?.Trim() : null,
                GuestPhone = isStaff ? dto.GuestPhone?.Trim() : null,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var added = await _reservationRepository.TryAddAsync(reservation);
            if (!added)
            {
                return Fail(ServiceErrorType.Conflict,
                    $"Table {table.TableNumber} is already reserved for the selected time.");
            }

            var saved = await _reservationRepository.GetByIdWithDetailsAsync(reservation.Id);
            return ServiceResult<ReservationDTO>.Ok(ToReservationDto(saved ?? reservation));
        }

        public async Task<List<ReservationDTO>> GetMineAsync(string userId)
        {
            var reservations = await _reservationRepository.GetByUserWithDetailsAsync(userId);
            return reservations.Select(ToReservationDto).ToList();
        }

        public async Task<List<ReservationDTO>> GetAllAsync(DateTimeOffset? fromTime, DateTimeOffset? toTime)
        {
            var reservations = await _reservationRepository.GetAllWithDetailsAsync(fromTime, toTime);
            return reservations.Select(ToReservationDto).ToList();
        }

        public async Task<ServiceResult<ReservationDTO>> GetByIdAsync(int id, string userId, bool isStaff)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return Fail(ServiceErrorType.NotFound, "Reservation not found.");
            }

            if (!isStaff && reservation.UserId != userId)
            {
                return Fail(ServiceErrorType.Forbidden, "You can only view your own reservations.");
            }

            return ServiceResult<ReservationDTO>.Ok(ToReservationDto(reservation));
        }

        public async Task<ServiceResult<ReservationDTO>> CancelAsync(int id, string userId, bool isStaff)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return Fail(ServiceErrorType.NotFound, "Reservation not found.");
            }

            if (!isStaff && reservation.UserId != userId)
            {
                return Fail(ServiceErrorType.Forbidden, "You can only cancel your own reservations.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return Fail(ServiceErrorType.Validation, "Reservation is already cancelled.");
            }

            await _reservationRepository.SetStatusAsync(reservation, ReservationStatus.Cancelled);
            return ServiceResult<ReservationDTO>.Ok(ToReservationDto(reservation));
        }

        public async Task<ServiceResult<ReservationDTO>> UpdateAsync(int id, string userId, UpdateReservationDTO dto, bool isStaff)
        {
            var validationError = Validate(dto.StartTime, dto.GuestCount, dto.DurationMinutes);
            if (validationError != null)
            {
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.Validation, validationError);
            }

            var guestValidation = ValidateGuestDetails(dto.GuestName, dto.GuestPhone, isStaff);
            if (guestValidation != null)
            {
                return Fail(ServiceErrorType.Validation, guestValidation);
            }
            if (dto.Note?.Trim().Length > MaxNoteLength)
            {
                return Fail(ServiceErrorType.Validation, $"Note can be at most {MaxNoteLength} characters.");
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.NotFound, "Reservation not found.");
            }

            if (!isStaff && reservation.UserId != userId)
            {
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.Forbidden, "You do not have permission to update this reservation.");
            }

            var table = await _tableRepository.GetByIdAsync(dto.TableId);
            if (table == null || !table.IsActive)
            {
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.NotFound, "Selected table does not exist or is inactive.");
            }

            if (table.Capacity < dto.GuestCount)
            {
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.Validation, $"Table capacity ({table.Capacity}) is less than guest count ({dto.GuestCount}).");
            }

            var oldStatus = reservation.Status;
            var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

            await _reservationRepository.SetStatusAsync(reservation, ReservationStatus.Cancelled);

            var availableTables = await _reservationRepository.GetAvailableTablesAsync(dto.StartTime, endTime, dto.GuestCount);
            if (!availableTables.Any(t => t.Id == dto.TableId))
            {
                await _reservationRepository.SetStatusAsync(reservation, oldStatus);
                return ServiceResult<ReservationDTO>.Fail(ServiceErrorType.Conflict, "The table is already reserved for this time slot.");
            }

            reservation.TableId = dto.TableId;
            reservation.StartTime = dto.StartTime;
            reservation.EndTime = endTime;
            reservation.GuestCount = dto.GuestCount;
            reservation.Table = table;
            if (isStaff)
            {
                reservation.GuestName = dto.GuestName?.Trim();
                reservation.GuestPhone = dto.GuestPhone?.Trim();
            }
            reservation.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            reservation.Status = ReservationStatus.Confirmed;

            await _reservationRepository.UpdateAsync(reservation);

            var saved = await _reservationRepository.GetByIdWithDetailsAsync(reservation.Id);
            return ServiceResult<ReservationDTO>.Ok(ToReservationDto(saved ?? reservation));
        }


        // Only staff may enter details for a guest who is different from the signed-in user.
        // These are required for new staff bookings, but remain nullable in the DB for legacy rows.
        private static string? ValidateGuestDetails(string? name, string? phone, bool isStaff)
        {
            if (!isStaff)
            {
                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(phone))
                    return "Only staff can assign guest contact details.";
                return null;
            }

            var cleanName = name?.Trim() ?? "";
            var cleanPhone = phone?.Trim() ?? "";
            if (cleanName.Length < 2 || cleanName.Length > 100)
                return "Guest name must contain 2 to 100 characters.";
            if (cleanPhone.Length < 7 || cleanPhone.Length > 25)
                return "Guest phone must contain 7 to 25 characters.";
            if (!cleanPhone.All(c => char.IsDigit(c) || c == '+' || c == ' ' || c == '-' || c == '(' || c == ')'))
                return "Guest phone contains unsupported characters.";
            if (cleanPhone.Count(char.IsDigit) < 7)
                return "Guest phone must contain at least 7 digits.";
            return null;
        }

        private static string? Validate(DateTimeOffset startTime, int guestCount, int durationMinutes)
        {
            if (guestCount < 1)
            {
                return "Guest count must be at least 1.";
            }

            if (durationMinutes < MinDurationMinutes || durationMinutes > MaxDurationMinutes)
            {
                return $"Duration must be between {MinDurationMinutes} and {MaxDurationMinutes} minutes.";
            }

            if (startTime <= DateTimeOffset.UtcNow)
            {
                return "Reservation time must be in the future.";
            }

            return null;
        }

        private static ServiceResult<ReservationDTO> Fail(ServiceErrorType errorType, string message)
        {
            return ServiceResult<ReservationDTO>.Fail(errorType, message);
        }

        private static TableDTO ToTableDto(Table table)
        {
            return new TableDTO
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        private static ReservationDTO ToReservationDto(Reservation reservation)
        {
            var customerName = reservation.User == null
                ? string.Empty
                : $"{reservation.User.FirstName} {reservation.User.LastName}".Trim();

            return new ReservationDTO
            {
                Id = reservation.Id,
                TableId = reservation.TableId,
                TableNumber = reservation.Table?.TableNumber ?? 0,
                UserId = reservation.UserId,
                CustomerName = !string.IsNullOrWhiteSpace(reservation.GuestName) ? reservation.GuestName : customerName,
                CustomerEmail = !string.IsNullOrWhiteSpace(reservation.GuestName) ? null : reservation.User?.Email,
                GuestName = reservation.GuestName,
                GuestPhone = reservation.GuestPhone,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                DurationMinutes = (int)(reservation.EndTime - reservation.StartTime).TotalMinutes,
                GuestCount = reservation.GuestCount,
                Note = reservation.Note,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }
    }
}