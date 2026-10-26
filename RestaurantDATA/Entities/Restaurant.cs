using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Entities
{

    public class Restaurant
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RestaurantUser> RestaurantUsers { get; set; }
            = new List<RestaurantUser>();
    }

}
