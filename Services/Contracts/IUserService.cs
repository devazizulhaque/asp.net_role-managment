using webapplication.Models.DTOs;

namespace webapplication.Services.Contracts
{
    public interface IUserService
    {
        Task<RegisterDto> RegisterUserAsync(RegisterDto dto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByEmailAsync(string email);
    }
}
