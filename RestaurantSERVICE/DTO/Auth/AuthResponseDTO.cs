using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.DTO.Auth
{
    public class AuthResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
