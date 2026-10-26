using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{

    [Table("Tables")]
    public class Table
    {
        [Key]
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsOccupied { get; set; } = false;
        public TableZone Zone { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public Guid? RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public TableStatus Status { get; set; } = TableStatus.Free;
    }
    public enum TableStatus
    {
        Free = 1,
        Occupied = 2,
        Reserved = 3
    }

    public enum TableZone
    {
        MainHall = 1,   
        Terrace = 2,   
        VIP = 3,        
        Balcony = 4
    }
}
