using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Table
{
    public class TableDTO
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; }
    }
}
