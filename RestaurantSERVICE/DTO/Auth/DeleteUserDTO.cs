using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Auth
{
    public class DeleteUserDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
