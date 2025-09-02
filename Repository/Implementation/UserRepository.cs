using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webapplication.Data;
using webapplication.Models.Entities;
using webapplication.Repository.Contracts;

namespace webapplication.Repository.Implementation
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        ) : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }
        public async Task<bool> AssignRoleAsync(ApplicationUser user, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
    }
}
