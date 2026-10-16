using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantDATA.Constants;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Auth;
using RestaurantSERVICE.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantSERVICE
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
            {
                return new AuthResponseDTO { IsSuccess = false, Message = "User with this email already exists" };
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDTO { IsSuccess = false, Message = errors };
            }

            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return new AuthResponseDTO { IsSuccess = true, Message = "Registration completed successfully" };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return new AuthResponseDTO { IsSuccess = false, Message = "Invalid email or password" };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token
            };
        }


        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, AppRoles.Normalize(role) ?? role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:DurationInDays"]!)),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync(); 
            var userList = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return userList;
        }

        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}

