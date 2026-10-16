using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Auth
{
    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
