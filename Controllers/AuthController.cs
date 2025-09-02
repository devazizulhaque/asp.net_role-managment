using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webapplication.Models.DTOs;
using webapplication.Services.Contracts;

namespace webapplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { message = "Password and Confirm Password do not match." });
            }
            try
            {
                var result = await _authService.RegisterAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _authService.LoginAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { message = "Email and Role are required." });

            var result = await _authService.AssignRoleAsync(request.Email, request.Role);

            if (!result)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "Role assigned successfully." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyInfo()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User email not found in token." });

            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
                return Unauthorized(new { message = "User not found." });
            return Ok(user);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var result = await _authService.RevokeTokenAsync(refreshToken);
            if (!result)
                return BadRequest(new { message = "Invalid refresh token." });
            return Ok(new { message = "Refresh token revoked." });
        }
    }
}