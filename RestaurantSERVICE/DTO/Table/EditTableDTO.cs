using System.ComponentModel.DataAnnotations;
using RestaurantDATA.Entities;

namespace RestaurantSERVICE.DTO.Table
{
    public class EditTableDTO
    {
        [Range(1, 1000)]
        public int TableNumber { get; set; }
        [Range(1, 50)]
        public int Capacity { get; set; }
        [EnumDataType(typeof(TableZone))]
        public TableZone Zone { get; set; } = TableZone.MainHall;
    }
}
