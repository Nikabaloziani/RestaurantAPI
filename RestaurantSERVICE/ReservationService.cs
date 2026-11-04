using System.Data;
using Microsoft.EntityFrameworkCore;
using RestaurantDAL.Repositories;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Common;
using RestaurantSERVICE.DTO.Reservations;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE;

// All reads and writes are restaurant-scoped. Customer and staff permissions
// come from the Controller's verified restaurant membership, never the request body.
public sealed class ReservationService : IReservationService
{
    private const int MinDurationMinutes = 30;
    private const int MaxDurationMinutes = 360;
    private const int MaxGuestCount = 50;
    private const int MaxNoteLength = 500;

    private readonly RestaurantDB _db;
    private readonly IReservationRepository _repository;

    public ReservationService(RestaurantDB db, IReservationRepository repository)
    {
        _db = db;
        _repository = repository;
    }

    public async Task<ServiceResult<List<TableDTO>>> GetAvailableTablesAsync(
        Guid restaurantId, DateTimeOffset startTime, int guestCount,
        int durationMinutes, CancellationToken ct = default)
    {
        var error = Validate(startTime, guestCount, durationMinutes);
        if (error != null)
            return ServiceResult<List<TableDTO>>.Fail(ServiceErrorType.Validation, error);

        var end = startTime.AddMinutes(durationMinutes);
        var tables = await _db.Tables.AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId && t.IsActive && !t.IsDeleted &&
                        t.Capacity >= guestCount && t.Restaurant != null && t.Restaurant.IsActive)
            .Where(t => !_db.Reservations.Any(r =>
                r.RestaurantId == restaurantId && r.TableId == t.Id &&
                r.Status == ReservationStatus.Confirmed &&
                r.StartTime < end && r.EndTime > startTime))
            .OrderBy(t => t.Capacity).ThenBy(t => t.TableNumber)
            .Select(t => new TableDTO
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Status = t.Status
            }).ToListAsync(ct);
        return ServiceResult<List<TableDTO>>.Ok(tables);
    }

    public async Task<ServiceResult<ReservationDTO>> CreateAsync(
        Guid restaurantId, string userId, CreateReservationDTO dto,
        bool isStaff, CancellationToken ct = default)
    {
        var error = Validate(dto.StartTime, dto.GuestCount, dto.DurationMinutes);
        if (error != null) return Fail(ServiceErrorType.Validation, error);

        error = ValidateNote(dto.Note);
        if (error != null) return Fail(ServiceErrorType.Validation, error);

        error = ValidateGuestDetails(dto.GuestName, dto.GuestPhone, isStaff);
        if (error != null) return Fail(ServiceErrorType.Validation, error);

        var table = await _db.Tables.AsNoTracking().FirstOrDefaultAsync(t =>
            t.Id == dto.TableId && t.RestaurantId == restaurantId &&
            t.Restaurant != null && t.Restaurant.IsActive && t.IsActive && !t.IsDeleted, ct);
        if (table == null) return Fail(ServiceErrorType.NotFound, "Table not found in this restaurant.");
        if (table.Capacity < dto.GuestCount)
            return Fail(ServiceErrorType.Validation, "Guest count exceeds table capacity.");

        var booking = new Reservation
        {
            RestaurantId = restaurantId, // Mandatory tenant ownership.
            TableId = table.Id,
            UserId = userId,
            StartTime = dto.StartTime,
            EndTime = dto.StartTime.AddMinutes(dto.DurationMinutes),
            GuestCount = dto.GuestCount,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            GuestName = isStaff ? dto.GuestName?.Trim() : null,
            GuestPhone = isStaff ? dto.GuestPhone?.Trim() : null,
            Status = ReservationStatus.Confirmed,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Reuses the repository's SERIALIZABLE reservation collision check.
        if (!await _repository.TryAddAsync(booking, ct))
            return Fail(ServiceErrorType.Conflict, "Table is already reserved for the selected time.");

        var saved = await ScopedReservations(restaurantId)
            .FirstAsync(x => x.Id == booking.Id, ct);
        return ServiceResult<ReservationDTO>.Ok(ToDto(saved));
    }

    public async Task<List<ReservationDTO>> GetMineAsync(
        Guid restaurantId, string userId, CancellationToken ct = default)
    {
        // My = self-booked customer reservations. Staff-created guest bookings
        // must be accessed via the staff-only GetAll endpoint.
        var records = await ScopedReservations(restaurantId).AsNoTracking()
            .Where(r => r.UserId == userId && r.GuestName == null)
            .OrderByDescending(r => r.StartTime).ToListAsync(ct);
        return records.Select(ToDto).ToList();
    }

    public async Task<List<ReservationDTO>> GetAllAsync(
        Guid restaurantId, DateTimeOffset? fromTime,
        DateTimeOffset? toTime, CancellationToken ct = default)
    {
        var query = ScopedReservations(restaurantId).AsNoTracking();
        if (fromTime.HasValue) query = query.Where(r => r.EndTime > fromTime.Value);
        if (toTime.HasValue) query = query.Where(r => r.StartTime < toTime.Value);
        var records = await query.OrderBy(r => r.StartTime).ToListAsync(ct);
        return records.Select(ToDto).ToList();
    }

    public async Task<ServiceResult<ReservationDTO>> GetByIdAsync(
        Guid restaurantId, int id, string userId, bool isStaff, CancellationToken ct = default)
    {
        var record = await ScopedReservations(restaurantId).AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (record == null) return Fail(ServiceErrorType.NotFound, "Reservation not found.");
        if (!CanAccess(record, userId, isStaff))
            return Fail(ServiceErrorType.Forbidden, "Access denied.");
        return ServiceResult<ReservationDTO>.Ok(ToDto(record));
    }

    public async Task<ServiceResult<ReservationDTO>> CancelAsync(
        Guid restaurantId, int id, string userId, bool isStaff, CancellationToken ct = default)
    {
        var record = await ScopedReservations(restaurantId)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (record == null) return Fail(ServiceErrorType.NotFound, "Reservation not found.");
        if (!CanAccess(record, userId, isStaff))
            return Fail(ServiceErrorType.Forbidden, "Access denied.");
        if (record.Status == ReservationStatus.Cancelled)
            return Fail(ServiceErrorType.Validation, "Reservation is already cancelled.");
        record.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
        return ServiceResult<ReservationDTO>.Ok(ToDto(record));
    }

    public async Task<ServiceResult<ReservationDTO>> UpdateAsync(
        Guid restaurantId, int id, string userId, UpdateReservationDTO dto,
        bool isStaff, CancellationToken ct = default)
    {
        var error = Validate(dto.StartTime, dto.GuestCount, dto.DurationMinutes);
        if (error != null) return Fail(ServiceErrorType.Validation, error);
        error = ValidateNote(dto.Note);
        if (error != null) return Fail(ServiceErrorType.Validation, error);
        error = ValidateGuestDetails(dto.GuestName, dto.GuestPhone, isStaff);
        if (error != null) return Fail(ServiceErrorType.Validation, error);

        // Both the overlap check and modification happen in one transaction.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var record = await ScopedReservations(restaurantId)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (record == null) return Fail(ServiceErrorType.NotFound, "Reservation not found.");
        if (!CanAccess(record, userId, isStaff))
            return Fail(ServiceErrorType.Forbidden, "Access denied.");
        if (record.Status == ReservationStatus.Cancelled)
            return Fail(ServiceErrorType.Validation, "Cancelled reservations cannot be edited.");

        var table = await _db.Tables.FirstOrDefaultAsync(t =>
            t.Id == dto.TableId && t.RestaurantId == restaurantId &&
            t.Restaurant != null && t.Restaurant.IsActive && t.IsActive && !t.IsDeleted, ct);
        if (table == null) return Fail(ServiceErrorType.NotFound, "Table not found in this restaurant.");
        if (table.Capacity < dto.GuestCount)
            return Fail(ServiceErrorType.Validation, "Guest count exceeds table capacity.");

        var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);
        var overlap = await _db.Reservations.AsNoTracking().AnyAsync(r =>
            r.Id != id && r.RestaurantId == restaurantId && r.TableId == dto.TableId &&
            r.Status == ReservationStatus.Confirmed &&
            r.StartTime < endTime && r.EndTime > dto.StartTime, ct);
        if (overlap)
            return Fail(ServiceErrorType.Conflict, "Table is already reserved for the selected time.");

        record.TableId = table.Id;
        record.Table = table;
        record.StartTime = dto.StartTime;
        record.EndTime = endTime;
        record.GuestCount = dto.GuestCount;
        record.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
        if (isStaff)
        {
            record.GuestName = dto.GuestName?.Trim();
            record.GuestPhone = dto.GuestPhone?.Trim();
        }
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return ServiceResult<ReservationDTO>.Ok(ToDto(record));
    }

    private IQueryable<Reservation> ScopedReservations(Guid restaurantId) =>
        _db.Reservations.Include(r => r.Table).Include(r => r.User)
            .Where(r => r.RestaurantId == restaurantId &&
                        r.Table.RestaurantId == restaurantId);

    private static bool CanAccess(Reservation reservation, string userId, bool isStaff) =>
        isStaff || (reservation.UserId == userId && reservation.GuestName == null);

    private static string? Validate(DateTimeOffset start, int guests, int duration)
    {
        if (start <= DateTimeOffset.UtcNow) return "Reservation time must be in the future.";
        if (guests is < 1 or > MaxGuestCount) return "Guest count must be between 1 and 50.";
        if (duration is < MinDurationMinutes or > MaxDurationMinutes)
            return "Duration must be between 30 and 360 minutes.";
        return null;
    }

    private static string? ValidateNote(string? note) =>
        note?.Trim().Length > MaxNoteLength ? "Note exceeds 500 characters." : null;

    private static string? ValidateGuestDetails(string? name, string? phone, bool isStaff)
    {
        if (!isStaff)
            return !string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(phone)
                ? "Only restaurant staff can set guest contact details." : null;

        var n = name?.Trim() ?? "";
        var p = phone?.Trim() ?? "";
        if (n.Length is < 2 or > 100) return "Guest name must contain 2 to 100 characters.";
        if (p.Length is < 7 or > 25 || p.Count(char.IsDigit) < 7 ||
            !p.All(ch => char.IsDigit(ch) || ch is '+' or ' ' or '-' or '(' or ')'))
            return "Guest phone is invalid.";
        return null;
    }

    private static ServiceResult<ReservationDTO> Fail(ServiceErrorType type, string message) =>
        ServiceResult<ReservationDTO>.Fail(type, message);

    private static ReservationDTO ToDto(Reservation r)
    {
        var displayName = r.User == null ? "" : $"{r.User.FirstName} {r.User.LastName}".Trim();
        return new ReservationDTO
        {
            Id = r.Id,
            TableId = r.TableId,
            TableNumber = r.Table?.TableNumber ?? 0,
            UserId = r.UserId,
            CustomerName = !string.IsNullOrWhiteSpace(r.GuestName) ? r.GuestName : displayName,
            CustomerEmail = !string.IsNullOrWhiteSpace(r.GuestName) ? null : r.User?.Email,
            GuestName = r.GuestName,
            GuestPhone = r.GuestPhone,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            DurationMinutes = (int)(r.EndTime - r.StartTime).TotalMinutes,
            GuestCount = r.GuestCount,
            Note = r.Note,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        };
    }
}
