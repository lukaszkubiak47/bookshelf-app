using Bookshelf.API.Models.DTOs.Auth;
using Bookshelf.API.RateLimiting;
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
        [RateLimit("register", limit: 5, windowSeconds: 3600)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Created("", result);
        }

        /// <summary>
        /// Logowanie: zwraca JWT token
        /// </summary>
        [HttpPost("login")]
        [RateLimit("login", limit: 10, windowSeconds: 300)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Weryfikacja danych przed resetem hasla
        /// </summary>
        [HttpPost("verify-password-reset")]
        [RateLimit("password-reset", limit: 5, windowSeconds: 900)] // wspólny licznik z reset-password: miesiąc założenia konta łatwo zgadnąć
        public async Task<IActionResult> VerifyPasswordReset([FromBody] VerifyPasswordResetDto dto)
        {
            await _authService.VerifyPasswordResetAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Reset hasla po weryfikacji emaila, nicku i miesiaca zalozenia konta
        /// </summary>
        [HttpPost("reset-password")]
        [RateLimit("password-reset", limit: 5, windowSeconds: 900)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return NoContent();
        }
    }
}
