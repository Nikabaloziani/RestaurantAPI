using RestaurantDATA.Entities;

namespace RestaurantSERVICE.DTO.Reservations
{
    public class ReservationDTO
    {
        public int Id { get; set; }

        public int TableId { get; set; }
        public int TableNumber { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int DurationMinutes { get; set; }

        public int GuestCount { get; set; }
        public string? Note { get; set; }

        /// <summary>1 = Confirmed, 2 = Cancelled</summary>
        public ReservationStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
