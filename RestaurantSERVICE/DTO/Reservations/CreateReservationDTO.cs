namespace RestaurantSERVICE.DTO.Reservations
{
    public class CreateReservationDTO
    {
        public int TableId { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public int GuestCount { get; set; }

        public int DurationMinutes { get; set; } = 120;

        public string? Note { get; set; }
    }
}
