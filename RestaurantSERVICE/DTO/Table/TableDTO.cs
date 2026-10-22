using RestaurantDATA.Entities;

namespace RestaurantSERVICE.DTO.Table
{
    public class TableDTO
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsOccupied { get; set; }
        public TableZone Zone { get; set; }
        public TableStatus Status { get; set; }
    }
}
