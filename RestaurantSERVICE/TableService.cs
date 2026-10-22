using AutoMapper;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantSERVICE
{
    public class TableService : ITableService
    {
        private readonly IBaseRepository<Table> _tableRepository;
        private readonly IMapper _mapper;

        public TableService(IBaseRepository<Table> tableRepository, IMapper mapper)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
        }

        public async Task<List<TableDTO>> GetAllTablesAsync()
        {
            var tables = await _tableRepository.GetAllAsync();
            var active = tables.Where(t => !t.IsDeleted).ToList();
            return _mapper.Map<List<TableDTO>>(active);
        }

        public async Task<TableDTO?> GetByIdAsync(int id)
        {
            var table = await _tableRepository.GetByIdAsync(id);
            return table == null || table.IsDeleted ? null : _mapper.Map<TableDTO>(table);
        }

        public async Task AddAsync(CreateTableDTO dto)
        {
            var entity = _mapper.Map<Table>(dto);
            entity.IsOccupied = false;
            entity.Status = TableStatus.Free;
            await _tableRepository.AddAsync(entity);
        }

        public async Task<bool> UpdateDetailsAsync(int id, EditTableDTO dto)
        {
            var table = await _tableRepository.GetByIdAsync(id);
            if (table == null || table.IsDeleted) return false;
            table.TableNumber = dto.TableNumber;
            table.Capacity = dto.Capacity;
            table.Zone = dto.Zone;
            await _tableRepository.UpdateAsync(table);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int tableId, bool isOccupied)
        {
            var table = await _tableRepository.GetByIdAsync(tableId);
            if (table == null || table.IsDeleted || !table.IsActive) return false;
            // Keep both fields consistent for UI and other services.
            table.IsOccupied = isOccupied;
            table.Status = isOccupied ? TableStatus.Occupied : TableStatus.Free;
            await _tableRepository.UpdateAsync(table);
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var table = await _tableRepository.GetByIdAsync(id);
            if (table == null || table.IsDeleted) return;
            // Soft delete to protect existing order/reservation foreign keys.
            table.IsDeleted = true;
            table.IsActive = false;
            await _tableRepository.UpdateAsync(table);
        }
    }
}
