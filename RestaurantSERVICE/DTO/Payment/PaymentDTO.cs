using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Payment
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
    }
}
