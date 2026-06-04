using Bookshelf.API.Models.DTOs.Auth;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Rejestracja nowego użytkownika
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Created("", result);
        }

        /// <summary>
        /// Logowanie: zwraca JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Weryfikacja danych przed resetem hasla
        /// </summary>
        [HttpPost("verify-password-reset")]
        public async Task<IActionResult> VerifyPasswordReset([FromBody] VerifyPasswordResetDto dto)
        {
            await _authService.VerifyPasswordResetAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Reset hasla po weryfikacji emaila, nicku i miesiaca zalozenia konta
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return NoContent();
        }
    }
}
