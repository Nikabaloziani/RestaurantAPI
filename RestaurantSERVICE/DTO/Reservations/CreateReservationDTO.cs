namespace RestaurantSERVICE.DTO.Reservations
{
    public class CreateReservationDTO
    {
        public int TableId { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public int GuestCount { get; set; }

        public int DurationMinutes { get; set; } = 120;

        public string? Note { get; set; }

        // For staff-created walk-in/customer bookings. Non-staff cannot override identity.
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
    }
}
