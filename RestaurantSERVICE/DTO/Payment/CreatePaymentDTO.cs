using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Payment
{
    public class CreatePaymentDTO
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
    }
}
