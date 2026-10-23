using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RestaurantSERVICE.DTO.Reservations
{
    public class UpdateReservationDTO
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTimeOffset StartTime { get; set; }

        [Range(1, 50, ErrorMessage = "Guest count must be between 1 and 50.")]
        public int GuestCount { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes.")]
        public int DurationMinutes { get; set; }

        public string? Note { get; set; }

        // For staff-created walk-in/customer bookings. Non-staff cannot override identity.
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
    }
}
