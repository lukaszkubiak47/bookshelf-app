using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services.Recognition
{
    // Zamiennik, gdy brak DocumentIntelligence:Endpoint/Key (lokalny dev): API działa, rozpoznawanie zwraca 503.
    public class UnavailableRecognitionService : IBookRecognitionService
    {
        public Task<BookRecognitionResultDto> RecognizeAsync(IFormFile file) =>
            throw new ServiceUnavailableException("Rozpoznawanie ze zdjęcia jest niedostępne: brak konfiguracji Document Intelligence");
    }
}
