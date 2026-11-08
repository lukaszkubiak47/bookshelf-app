using Bookshelf.API.Models.DTOs.Users;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Pobierz profil zalogowanego użytkownika.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // Pobierz ID użytkownika z JWT i przekaż je do serwisu
            var userId = GetCurrentUserId();
            var result = await _service.GetMeAsync(userId);

            // Zwróć DTO bez PasswordHash
            return Ok(result);
        }

        /// <summary>
        /// Aktualizuj dane profilu zalogowanego użytkownika.
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
        {
            // Pobierz ID użytkownika z JWT
            var userId = GetCurrentUserId();

            // Serwis sprawdzi walidacje biznesowe, np. zajęty username
            var result = await _service.UpdateMeAsync(userId, dto);

            return Ok(result);
        }

        /// <summary>
        /// Zmień hasło zalogowanego użytkownika.
        /// </summary>
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Pobierz ID użytkownika z JWT
            var userId = GetCurrentUserId();

            // Serwis sprawdza aktualne hasło i zapisuje nowe jako hash BCrypt
            await _service.ChangePasswordAsync(userId, dto);

            return NoContent();
        }

        /// <summary>
        /// Upload własnego avatara jako danych binarnych.
        /// </summary>
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            // Pobierz ID użytkownika z JWT
            var userId = GetCurrentUserId();

            // Serwis waliduje plik i zapisuje go w AvatarImageData
            var result = await _service.UploadAvatarAsync(userId, file);

            return Ok(result);
        }

        /// <summary>
        /// Wybierz avatar z gotowych presetów.
        /// </summary>
        [HttpPut("me/avatar-preset")]
        public async Task<IActionResult> SetAvatarPreset([FromBody] AvatarPresetDto dto)
        {
            // Pobierz ID użytkownika z JWT
            var userId = GetCurrentUserId();

            // Serwis ustawia ścieżkę do presetu i czyści ewentualny avatar binarny
            var result = await _service.SetAvatarPresetAsync(userId, dto);

            return Ok(result);
        }

        /// <summary>
        /// Avatar dowolnego użytkownika: przekierowanie 302 na krótki link SAS do Blob Storage.
        /// Anonimowy, bo <img> nie wysyła tokenu.
        /// </summary>
        [HttpGet("{id:guid}/avatar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatar(Guid id)
        {
            var uri = await _service.GetAvatarReadUriAsync(id);
            return Redirect(uri.ToString());
        }

        private Guid GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier jest ustawiany podczas generowania JWT w TokenService
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
