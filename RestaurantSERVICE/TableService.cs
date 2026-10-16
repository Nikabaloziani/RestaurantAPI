using AutoMapper;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;
using RestaurantSERVICE.DTO.Table;
using RestaurantSERVICE.Interfaces;
using System.Collections.Generic;
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
            return _mapper.Map<List<TableDTO>>(tables);
        }

        public async Task<TableDTO?> GetByIdAsync(int id)
        {
            var table = await _tableRepository.GetByIdAsync(id);
            return table == null ? null : _mapper.Map<TableDTO>(table);
        }

        public async Task AddAsync(CreateTableDTO dto)
        {
            var entity = _mapper.Map<Table>(dto);
            await _tableRepository.AddAsync(entity);
        }

        public async Task<bool> UpdateStatusAsync(int tableId, bool isOccupied)
        {
            var table = await _tableRepository.GetByIdAsync(tableId);
            if (table == null)
            {
                return false;
            }

            table.IsOccupied = isOccupied;

            await _tableRepository.UpdateAsync(table);
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var table = await _tableRepository.GetByIdAsync(id);
            if (table != null)
            {
                await _tableRepository.DeleteAsync(table);
            }
        }
    }
}