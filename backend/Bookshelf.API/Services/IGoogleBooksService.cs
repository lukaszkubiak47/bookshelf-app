using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services
{
    public interface IGoogleBooksService
    {
        // Szuka ksiazek w Google Books API i zwraca wyniki w naszym DTO.
        Task<List<GoogleBookResultDto>> SearchAsync(string query);
    }
}
