using RestaurantDATA.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Entities
{

    public class RestaurantUser
    {
        public Guid RestaurantId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public RestaurantRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Restaurant Restaurant { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }

}
