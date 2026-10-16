using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;

namespace RestaurantDAL.Repositories
{
    public interface IReservationRepository : IBaseRepository<Reservation>
    {
        Task<Reservation?> GetByIdWithDetailsAsync(int id);

        Task<List<Reservation>> GetAllWithDetailsAsync(DateTimeOffset? fromTime, DateTimeOffset? toTime);

        Task<List<Reservation>> GetByUserWithDetailsAsync(string userId);

        Task<List<Table>> GetAvailableTablesAsync(DateTimeOffset start, DateTimeOffset end, int guestCount);

        Task<bool> HasUpcomingReservationsAsync(int tableId);


        Task<bool> TryAddAsync(Reservation reservation, CancellationToken cancellationToken = default);

        Task SetStatusAsync(Reservation reservation, ReservationStatus status);
    }

    public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(RestaurantDB context) : base(context) { }

        public async Task<Reservation?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Table)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reservation>> GetAllWithDetailsAsync(DateTimeOffset? fromTime, DateTimeOffset? toTime)
        {
            var query = _dbSet
                .Include(r => r.Table)
                .Include(r => r.User)
                .AsQueryable();

            if (fromTime.HasValue)
                query = query.Where(r => r.EndTime > fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(r => r.StartTime < toTime.Value);

            return await query
                .OrderBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<List<Reservation>> GetByUserWithDetailsAsync(string userId)
        {
            return await _dbSet
                .Include(r => r.Table)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<List<Table>> GetAvailableTablesAsync(DateTimeOffset start, DateTimeOffset end, int guestCount)
        {
            return await _context.Tables
                .Where(t => t.Capacity >= guestCount)
                .Where(t => !_context.Reservations.Any(r =>
                    r.TableId == t.Id &&
                    r.Status == ReservationStatus.Confirmed &&
                    r.StartTime < end &&
                    r.EndTime > start))
                .OrderBy(t => t.Capacity)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();
        }

        public async Task<bool> HasUpcomingReservationsAsync(int tableId)
        {
            var now = DateTimeOffset.UtcNow;

            return await _dbSet.AnyAsync(r =>
                r.TableId == tableId &&
                r.Status == ReservationStatus.Confirmed &&
                r.EndTime > now);
        }

        public async Task<bool> TryAddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;", cancellationToken);

            var hasOverlap = await _dbSet.AnyAsync(r =>
                r.TableId == reservation.TableId &&
                r.Status == ReservationStatus.Confirmed &&
                r.StartTime < reservation.EndTime &&
                r.EndTime > reservation.StartTime, cancellationToken);

            if (hasOverlap)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await _dbSet.AddAsync(reservation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        public async Task SetStatusAsync(Reservation reservation, ReservationStatus status)
        {
            reservation.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}
