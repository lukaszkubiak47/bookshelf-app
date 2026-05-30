using Bookshelf.API.Data;
using Bookshelf.API.Models.DTOs.Books;
using Bookshelf.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf.API.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _db;

        public BookService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<BookDto>> SearchAsync(string? search, int limit = 10)
        {
            if (limit > 20) limit = 20;

            var query = _db.Books
                .Include(b => b.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    (b.ISBN != null && b.ISBN.Contains(search)));
            }

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            Guid? genreId = null;
            if (!string.IsNullOrWhiteSpace(dto.GenreName))
            {
                var genre = await _db.Genres.FirstOrDefaultAsync(g => g.Name == dto.GenreName);
                genreId = genre?.Id;
            }

            var book = new Book
            {
                Title = dto.Title.Trim(),
                Author = dto.Author.Trim(),
                Description = dto.Description,
                ISBN = string.IsNullOrWhiteSpace(dto.ISBN) ? null : dto.ISBN.Trim(),
                GenreId = genreId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            await _db.Entry(book).Reference(b => b.Genre).LoadAsync();
            return MapToDto(book);
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                CoverUrl = book.CoverUrl,
                ISBN = book.ISBN,
                GenreName = book.Genre?.Name
            };
        }
    }
}
