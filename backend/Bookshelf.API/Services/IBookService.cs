using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services
{
    public interface IBookService
    {
        // Dodaje ksiazke do naszej bazy albo zwraca istniejaca, jesli GoogleBooksId juz istnieje.
        Task<BookDto> CreateAsync(CreateBookDto dto);

        // Pobiera slownik gatunkow z bazy.
        Task<List<GenreDto>> GetGenresAsync();
    }
}
