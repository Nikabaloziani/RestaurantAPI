using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Entities
{
    public enum OrderStatus
    {
        Pending = 1,    // მიღებულია
        Preparing = 2,  // მზადდება
        Ready = 3,      // მზადაა
        Served = 4,     // მიწოდებულია
        Paid = 5,       // გადახდილია
        Cancelled = 6   // გაუქმებულია
    }
}
