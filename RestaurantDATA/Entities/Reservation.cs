namespace RestaurantDATA.Entities
{
    public enum ReservationStatus
    {
        Confirmed = 1,
        Cancelled = 2
    }

    public class Reservation
    {
        public int Id { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public int GuestCount { get; set; }
        public string? Note { get; set; }

        // Guest details are optional for existing reservations. UserId tracks who created the booking.
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
