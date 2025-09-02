using webapplication.Models.DTOs;

namespace webapplication.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<bool> AssignRoleAsync(string email, string role);
        Task<UserDto?> GetUserByEmailAsync(string email);

        // Add refresh and revoke token signatures
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}
