
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Enum;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class TableService : ITableService
    {
        private readonly RestaurantDB _context;
        private readonly IMapper _mapper;

        public TableService(
            RestaurantDB context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task EnsureAccessAsync(
            Guid restaurantId,
            string userId,
            CancellationToken cancellationToken,
            params RestaurantRole[] allowedRoles)
        {
            if (restaurantId == Guid.Empty ||
                string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Restaurant access denied.");
            }

            var role = await _context.RestaurantUsers
                .AsNoTracking()
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    x.UserId == userId &&
                    x.Restaurant.IsActive)
                .Select(x => (RestaurantRole?)x.Role)
                .SingleOrDefaultAsync(cancellationToken);

            if (!role.HasValue ||
                !allowedRoles.Contains(role.Value))
            {
                throw new UnauthorizedAccessException(
                    "Restaurant access denied.");
            }
        }

        public async Task<List<TableDTO>> GetAllTablesAsync(
            Guid restaurantId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager,
                RestaurantRole.Waiter,
                RestaurantRole.Cashier);

            var tables = await _context.Tables
                .AsNoTracking()
                .Where(t =>
                    t.RestaurantId == restaurantId &&
                    !t.IsDeleted)
                .OrderBy(t => t.TableNumber)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<TableDTO>>(tables);
        }

        public async Task<TableDTO?> GetByIdAsync(
            Guid restaurantId,
            int id,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager,
                RestaurantRole.Waiter,
                RestaurantRole.Cashier);

            var table = await _context.Tables
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t =>
                        t.Id == id &&
                        t.RestaurantId == restaurantId &&
                        !t.IsDeleted,
                    cancellationToken);

            return table == null
                ? null
                : _mapper.Map<TableDTO>(table);
        }

        public async Task AddAsync(
            Guid restaurantId,
            CreateTableDTO dto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager);

            var entity = _mapper.Map<Table>(dto);

            entity.RestaurantId = restaurantId;
            entity.IsOccupied = false;
            entity.Status = TableStatus.Free;
            entity.IsActive = true;
            entity.IsDeleted = false;

            _context.Tables.Add(entity);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<bool> UpdateDetailsAsync(
            Guid restaurantId,
            int id,
            EditTableDTO dto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager);

            var table = await _context.Tables
                .FirstOrDefaultAsync(
                    t =>
                        t.Id == id &&
                        t.RestaurantId == restaurantId &&
                        !t.IsDeleted,
                    cancellationToken);

            if (table == null)
                return false;

            table.TableNumber = dto.TableNumber;
            table.Capacity = dto.Capacity;
            table.Zone = dto.Zone;

            await _context.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<bool> UpdateStatusAsync(
            Guid restaurantId,
            int tableId,
            bool isOccupied,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager,
                RestaurantRole.Waiter);

            var table = await _context.Tables
                .FirstOrDefaultAsync(
                    t =>
                        t.Id == tableId &&
                        t.RestaurantId == restaurantId &&
                        !t.IsDeleted &&
                        t.IsActive,
                    cancellationToken);

            if (table == null)
                return false;

            table.IsOccupied = isOccupied;

            table.Status = isOccupied
                ? TableStatus.Occupied
                : TableStatus.Free;

            await _context.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            Guid restaurantId,
            int id,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessAsync(
                restaurantId,
                userId,
                cancellationToken,
                RestaurantRole.Owner,
                RestaurantRole.Manager);

            var table = await _context.Tables
                .FirstOrDefaultAsync(
                    t =>
                        t.Id == id &&
                        t.RestaurantId == restaurantId &&
                        !t.IsDeleted,
                    cancellationToken);

            if (table == null)
                return false;

            table.IsDeleted = true;
            table.IsActive = false;

            await _context.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }
}
