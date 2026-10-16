using RestaurantSERVICE.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSERVICE.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(string id);
        Task<bool> DeleteUserAsync(string id);

    }
}
