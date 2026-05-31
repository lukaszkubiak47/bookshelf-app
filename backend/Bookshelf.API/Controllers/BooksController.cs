using Bookshelf.API.Models.DTOs.Books;
using Bookshelf.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IGoogleBooksService _googleBooksService;
        private readonly IBookService _bookService;

        public BooksController(IGoogleBooksService googleBooksService, IBookService bookService)
        {
            _googleBooksService = googleBooksService;
            _bookService = bookService;
        }

        /// <summary>
        /// Szukaj ksiazek w Google Books API.
        /// </summary>
        [HttpGet("api/books/search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            // Serwis waliduje query, odpytuje Google i korzysta z cache.
            var result = await _googleBooksService.SearchAsync(q);
            return Ok(result);
        }

        /// <summary>
        /// Dodaj wybrana ksiazke do naszej bazy.
        /// </summary>
        [HttpPost("api/books")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            // Serwis pilnuje duplikatow po GoogleBooksId i waliduje GenreId.
            var result = await _bookService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }

        /// <summary>
        /// Pobierz liste gatunkow z bazy.
        /// </summary>
        [HttpGet("api/genres")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGenres()
        {
            // Gatunki sa publicznym slownikiem dla frontendu.
            var result = await _bookService.GetGenresAsync();
            return Ok(result);
        }
    }
}
