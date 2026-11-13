using Azure;
using Azure.AI.DocumentIntelligence;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Books;

namespace Bookshelf.API.Services.Recognition
{
    // Rozpoznanie książki ze zdjęcia okładki: Document Intelligence (model prebuilt-read) odczytuje tekst,
    // a pierwsze linie (zwykle tytuł i autor) idą jako zapytanie do Google Books (z cache w Redisie).
    public class DocumentIntelligenceRecognitionService : IBookRecognitionService
    {
        // Limit usługi to 4 MB na plik (F0); frontend zmniejsza zdjęcie przed wysłaniem.
        public const long MaxImageSizeBytes = 4 * 1024 * 1024;
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png"];
        private const int LinesForQuery = 3;
        private const int MaxQueryLength = 200;

        private readonly DocumentIntelligenceClient _client;
        private readonly IGoogleBooksService _googleBooks;
        private readonly ILogger<DocumentIntelligenceRecognitionService> _logger;

        public DocumentIntelligenceRecognitionService(
            DocumentIntelligenceClient client,
            IGoogleBooksService googleBooks,
            ILogger<DocumentIntelligenceRecognitionService> logger)
        {
            _client = client;
            _googleBooks = googleBooks;
            _logger = logger;
        }

        public async Task<BookRecognitionResultDto> RecognizeAsync(IFormFile file)
        {
            ValidateImage(file);

            List<string> lines;
            try
            {
                await using var stream = file.OpenReadStream();
                var operation = await _client.AnalyzeDocumentAsync(
                    WaitUntil.Completed, "prebuilt-read", await BinaryData.FromStreamAsync(stream));

                lines = operation.Value.Pages
                    .SelectMany(p => p.Lines)
                    .Select(l => l.Content.Trim())
                    .Where(t => t.Length > 2)
                    .ToList();
            }
            catch (RequestFailedException ex) when (ex.Status == 429)
            {
                throw new TooManyRequestsException("Wyczerpano limit rozpoznawania zdjęć. Spróbuj później albo wyszukaj tytuł ręcznie.");
            }
            catch (RequestFailedException ex)
            {
                // Bez komunikatu wyjątku: usługa może w nim zwracać szczegóły żądania.
                _logger.LogWarning("Document Intelligence zwróciło błąd {Status} ({Code})", ex.Status, ex.ErrorCode);
                throw new ServiceUnavailableException("Rozpoznawanie ze zdjęcia jest chwilowo niedostępne");
            }
            catch (AggregateException ex)
            {
                // Brak połączenia z usługą: SDK po ponowieniach zwraca AggregateException ("Retry failed after N tries")
                _logger.LogWarning("Brak połączenia z Document Intelligence ({Type})", ex.InnerException?.GetType().Name);
                throw new ServiceUnavailableException("Rozpoznawanie ze zdjęcia jest chwilowo niedostępne");
            }

            if (lines.Count == 0)
                throw new BadRequestException("Nie udało się odczytać tekstu ze zdjęcia. Zrób wyraźniejsze zdjęcie okładki albo wyszukaj tytuł ręcznie.");

            // Tytuł i autor są zwykle w pierwszych liniach okładki
            var query = string.Join(' ', lines.Take(LinesForQuery));
            if (query.Length > MaxQueryLength)
                query = query[..MaxQueryLength];

            var results = await _googleBooks.SearchAsync(query);
            return new BookRecognitionResultDto { Query = query, Results = results };
        }

        private static void ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("Zdjęcie jest wymagane");

            if (file.Length > MaxImageSizeBytes)
                throw new BadRequestException("Zdjęcie nie może przekraczać 4 MB");

            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new BadRequestException("Dozwolone formaty zdjęcia: JPG, PNG");
        }
    }
}
