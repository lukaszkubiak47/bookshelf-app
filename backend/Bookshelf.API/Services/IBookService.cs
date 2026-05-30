using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> SearchAsync(string? search, int limit = 10);
        Task<BookDto> CreateAsync(CreateBookDto dto);
    }
}
