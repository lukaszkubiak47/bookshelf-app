using Bookshelf.API.Models.DTOs.Books;
using Bookshelf.API.RateLimiting;
using Bookshelf.API.Services;
using Bookshelf.API.Services.Recognition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookshelf.API.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IGoogleBooksService _googleBooksService;
        private readonly IBookService _bookService;
        private readonly IBookRecognitionService _recognitionService;

        public BooksController(
            IGoogleBooksService googleBooksService,
            IBookService bookService,
            IBookRecognitionService recognitionService)
        {
            _googleBooksService = googleBooksService;
            _bookService = bookService;
            _recognitionService = recognitionService;
        }

        /// <summary>
        /// Rozpoznaj ksiazke ze zdjecia okladki (JPG/PNG do 4 MB): tekst z Document Intelligence -> Google Books.
        /// </summary>
        [HttpPost("recognize")]
        [Authorize]
        [RateLimit("book-recognize", limit: 10, windowSeconds: 3600)] // chroni darmowe 500 stron/mies. (F0)
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Recognize(IFormFile file)
        {
            var result = await _recognitionService.RecognizeAsync(file);
            return Ok(result);
        }

        /// <summary>
        /// Szukaj ksiazek w Google Books API.
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            // Serwis waliduje query, odpytuje Google i korzysta z cache.
            var result = await _googleBooksService.SearchAsync(q);
            return Ok(result);
        }

        /// <summary>
        /// Pobierz pojedyncza ksiazke z naszej bazy po Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Serwis rzuca NotFoundException, jesli ksiazki nie ma.
            var result = await _bookService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Dodaj wybrana ksiazke do naszej bazy.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            // Serwis pilnuje duplikatow po GoogleBooksId i waliduje GenreId.
            var result = await _bookService.CreateAsync(dto);
            // Location wskazuje na GET api/books/{id}.
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Pobierz liste gatunkow z bazy.
        /// </summary>
        // Trasa absolutna (wiodacy /), zeby zachowac /api/genres mimo [Route("api/books")] na klasie.
        [HttpGet("/api/genres")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGenres()
        {
            // Gatunki sa publicznym slownikiem dla frontendu.
            var result = await _bookService.GetGenresAsync();
            return Ok(result);
        }
    }
}
