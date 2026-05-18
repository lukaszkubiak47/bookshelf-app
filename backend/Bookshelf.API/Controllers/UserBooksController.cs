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
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.UpdateAsync(userId, id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Wystaw książkę na półce (wymaga JWT)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUserBookDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.CreateAsync(userId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Usuń swoją ofertę z półki (wymaga JWT)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.DeleteAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}