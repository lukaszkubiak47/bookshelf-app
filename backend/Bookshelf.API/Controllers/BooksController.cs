using Bookshelf.API.Models.DTOs.Books;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;

        public BooksController(IBookService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int limit = 10)
        {
            var result = await _service.SearchAsync(search, limit);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Created("", result);
        }
    }
}
