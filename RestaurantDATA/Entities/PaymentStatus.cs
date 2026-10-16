using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Entities
{
    public enum PaymentStatus
    {
        Pending = 1,    // მოლოდინშია
        Completed = 2,  // წარმატებული
        Failed = 3      // წარუმატებელი
    }
}
