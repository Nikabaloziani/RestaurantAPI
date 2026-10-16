using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RestaurantSERVICE.DTO.Table
{
    public class CreateTableDTO
    {
        [Range(1, 1000, ErrorMessage = "Table number must be between 1 and 1000")]
        public int TableNumber { get; set; }

        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50 guests")]
        public int Capacity { get; set; }
    }
}
