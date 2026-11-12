using Bookshelf.API.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Moje najnowsze powiadomienia i liczba nieprzeczytanych.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var result = await _service.GetForUserAsync(GetCurrentUserId());
            return Ok(result);
        }

        /// <summary>
        /// Oznacz powiadomienie jako przeczytane.
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(string id)
        {
            await _service.MarkReadAsync(GetCurrentUserId(), id);
            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier jest ustawiany podczas generowania JWT w TokenService
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
