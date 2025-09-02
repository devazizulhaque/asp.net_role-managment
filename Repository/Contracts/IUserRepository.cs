using webapplication.Models.Entities;

namespace webapplication.Repository.Contracts
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> RoleExistsAsync(string roleName);
        Task<bool> AssignRoleAsync(ApplicationUser user, string roleName);
    }
}
