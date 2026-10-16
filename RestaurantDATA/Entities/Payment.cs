using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RestaurantDATA.Entities
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? TransactionId { get; set; }
    }
}
