using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services.Recognition
{
    public interface IBookRecognitionService
    {
        /// <summary>Odczytuje tekst ze zdjęcia okładki i szuka go w Google Books.</summary>
        Task<BookRecognitionResultDto> RecognizeAsync(IFormFile file);
    }
}
