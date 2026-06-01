using Bookshelf.API.Data;
using Bookshelf.API.Exceptions;
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

        public async Task<BookDto> GetByIdAsync(Guid id)
        {
            // Pojedyncza ksiazka z naszej bazy (z gatunkiem do GenreName).
            var book = await _db.Books
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                throw new NotFoundException("Nie znaleziono ksiazki o podanym Id");

            return MapToDto(book);
        }

        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            // GenreId jest wymagany i musi byc niepusty (Guid.Empty oznacza brak wyboru w body).
            if (dto.GenreId is null || dto.GenreId == Guid.Empty)
                throw new BadRequestException("GenreId jest wymagany");

            var genreId = dto.GenreId.Value;

            // Jesli ksiazka z GoogleBooksId juz istnieje, nie tworzymy duplikatu.
            if (!string.IsNullOrWhiteSpace(dto.GoogleBooksId))
            {
                var existingBook = await _db.Books
                    .Include(b => b.Genre)
                    .FirstOrDefaultAsync(b => b.GoogleBooksId == dto.GoogleBooksId);

                if (existingBook != null)
                    return MapToDto(existingBook);
            }

            // Sprawdzamy czy wybrany gatunek istnieje w slowniku z seeda.
            var genre = await _db.Genres.FirstOrDefaultAsync(g => g.Id == genreId);

            if (genre == null)
                throw new NotFoundException("Nie znaleziono gatunku o podanym ID");

            // Tworzymy nowa ksiazke w naszej bazie.
            var book = new Book
            {
                GoogleBooksId = string.IsNullOrWhiteSpace(dto.GoogleBooksId) ? null : dto.GoogleBooksId,
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                GenreId = genreId,
                CoverUrl = dto.CoverUrl,
                ISBN = dto.Isbn,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _db.Books.Add(book);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException) when (!string.IsNullOrWhiteSpace(dto.GoogleBooksId))
            {
                // Wyscig: rownolegly request dodal te sama ksiazke (naruszenie unikalnego indeksu GoogleBooksId).
                // Zwracamy istniejaca pozycje zamiast 500.
                var existing = await _db.Books
                    .Include(b => b.Genre)
                    .FirstOrDefaultAsync(b => b.GoogleBooksId == dto.GoogleBooksId);

                if (existing != null)
                    return MapToDto(existing);

                throw;
            }

            // Dopinamy gatunek do encji, zeby DTO mialo GenreName bez dodatkowego zapytania.
            book.Genre = genre;

            return MapToDto(book);
        }

        public async Task<List<GenreDto>> GetGenresAsync()
        {
            // Gatunki sa seedowane przy migracji i sa publicznym slownikiem dla frontendu.
            return await _db.Genres
                .OrderBy(g => g.Name)
                .Select(g => new GenreDto
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync();
        }

        private static BookDto MapToDto(Book book)
        {
            // Mapujemy encje Book na DTO bez wystawiania encji EF bezposrednio.
            return new BookDto
            {
                Id = book.Id,
                GoogleBooksId = book.GoogleBooksId,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                GenreId = book.GenreId,
                GenreName = book.Genre?.Name ?? string.Empty,
                CoverUrl = book.CoverUrl,
                Isbn = book.ISBN,
                CreatedAt = book.CreatedAt
            };
        }
    }
}
