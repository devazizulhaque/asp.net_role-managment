using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using webapplication.Models.DTOs;
using webapplication.Models.Entities;
using webapplication.Repository.Contracts;
using webapplication.Services.Contracts;

namespace webapplication.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private static readonly Dictionary<string, string> _refreshTokens = new();

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _userRepository = userRepository;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            // Check if the user already exists
            if (await _userRepository.UserExistsAsync(model.Email))
                throw new Exception("User already exists!");

            // Create new user object
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");

            return await GenerateJwtToken(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
                throw new Exception("Invalid email or password!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid email or password!");

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return await GenerateJwtToken(user);
        }
        public async Task<bool> AssignRoleAsync(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Validate refresh token
            if (!_refreshTokens.ContainsKey(refreshToken))
                throw new Exception("Invalid refresh token.");

            var email = _refreshTokens[refreshToken];
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.IsActive)
                throw new Exception("User not found or inactive.");

            // Generate new tokens
            var response = await GenerateJwtToken(user);
            response.Token = GenerateRefreshToken(email); // Assign new refresh token
            return response;
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            // Run the removal on a background thread to satisfy async requirements
            return await Task.Run(() => _refreshTokens.Remove(refreshToken));
        }

        private string GenerateRefreshToken(string email)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);
            _refreshTokens[refreshToken] = email;
            return refreshToken;
        }

        private async Task<AuthResponseDto> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var response = new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                Roles = roles
            };
            response.Token = GenerateRefreshToken(user.Email!);
            return response;
        }
    }
}
