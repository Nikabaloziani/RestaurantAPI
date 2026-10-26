using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
        public interface ICurrentRestaurantService
        {
            Task<bool> HasAccessAsync( Guid restaurantId,string userId,CancellationToken cancellationToken = default);
        }
    }
