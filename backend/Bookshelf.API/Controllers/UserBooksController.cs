using Bookshelf.API.Models.DTOs.UserBooks;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/userbooks")]
    public class UserBooksController : ControllerBase
    {
        private readonly IUserBookService _service;

        public UserBooksController(IUserBookService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista ofert z filtrami i paginacją (publiczny)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 12,
            [FromQuery] string? type = null,
            [FromQuery] string? genre = null,
            [FromQuery] string? city = null,
            [FromQuery] string? search = null,
            [FromQuery] string sort = "newest")
        {
            var result = await _service.GetAllAsync(page, limit, type, genre, city, search, sort);
            return Ok(result);
        }

        /// <summary>
        /// Szczegóły jednej oferty (publiczny)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Nie znaleziono oferty" });
            return Ok(result);
        }

        /// <summary>
        /// Ostatnio dodane oferty (publiczny: strona główna)
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
        {
            if (limit > 20) limit = 20;
            var result = await _service.GetRecentAsync(limit);
            return Ok(result);
        }

        /// <summary>
        /// Moje wystawione oferty (wymaga JWT)
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMy()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetMyAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Edytuj swoją ofertę (wymaga JWT)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserBookDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.UpdateAsync(userId, id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Wgraj własną okładkę oferty (wymaga JWT, tylko właściciel)
        /// </summary>
        [HttpPost("{id:guid}/cover")]
        [Authorize]
        public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.UploadCoverAsync(userId, id, file);
            return Ok(result);
        }

        /// <summary>
        /// Pobierz własną okładkę oferty (publiczny)
        /// </summary>
        [HttpGet("{id:guid}/cover/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoverImage(Guid id, [FromQuery] string? size = null)
        {
            // ?size=thumb -> miniatura (jeśli już jest), bez parametru -> oryginał
            var uri = await _service.GetCoverReadUriAsync(id, thumbnail: size == "thumb");
            return Redirect(uri.ToString());
        }

        /// <summary>
        /// Usuń własną okładkę oferty (wymaga JWT, tylko właściciel)
        /// </summary>
        [HttpDelete("{id:guid}/cover")]
        [Authorize]
        public async Task<IActionResult> DeleteCover(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.DeleteCoverAsync(userId, id);
            return NoContent();
        }

        /// <summary>
        /// Wystaw książkę na półce (wymaga JWT)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUserBookDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Usuń swoją ofertę z półki (wymaga JWT)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.DeleteAsync(userId, id);
            return NoContent();
        }
    }
}