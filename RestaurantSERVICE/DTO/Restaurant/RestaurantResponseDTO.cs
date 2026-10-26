using RestaurantDATA.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Restaurant
{

    public class RestaurantResponseDTO
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public RestaurantRole Role { get; set; }
    }

}
