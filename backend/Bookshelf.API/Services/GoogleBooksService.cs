using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Books;
using Microsoft.Extensions.Caching.Memory;

namespace Bookshelf.API.Services
{
    public class GoogleBooksService : IGoogleBooksService
    {
        private const string GoogleBooksBaseUrl = "https://www.googleapis.com/books/v1/volumes";
        private const int MaxResults = 10;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        private readonly IMemoryCache _cache;
        private readonly HttpClient _http;

        public GoogleBooksService(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _http = httpClientFactory.CreateClient();
        }

        public async Task<List<GoogleBookResultDto>> SearchAsync(string query)
        {
            // Nie wysylamy do Google pustych albo zbyt krotkich zapytan.
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                throw new BadRequestException("Query musi miec co najmniej 2 znaki");

            var normalizedQuery = query.Trim();
            var cacheKey = $"google_books_{normalizedQuery.ToLowerInvariant()}";

            // Jesli to samo zapytanie bylo juz wykonane, zwroc wynik z pamieci.
            if (_cache.TryGetValue(cacheKey, out List<GoogleBookResultDto>? cachedResults))
                return cachedResults!;

            var url = $"{GoogleBooksBaseUrl}?q={Uri.EscapeDataString(normalizedQuery)}&maxResults={MaxResults}";

            // Google zwraca mocno zagniezdzony JSON, dlatego mapujemy go przez prywatne klasy odpowiedzi.
            var response = await _http.GetFromJsonAsync<GoogleBooksResponse>(url);
            var results = MapResults(response);

            // Cache na 24h ogranicza liczbe requestow do Google i przyspiesza powtarzane wyszukiwania.
            _cache.Set(cacheKey, results, CacheDuration);

            return results;
        }

        private static List<GoogleBookResultDto> MapResults(GoogleBooksResponse? response)
        {
            if (response?.Items == null)
                return new List<GoogleBookResultDto>();

            return response.Items
                .Where(item => item.VolumeInfo != null)
                .Select(item =>
                {
                    var volume = item.VolumeInfo!;

                    // Google nie zawsze zwraca autora, ISBN, opis albo okladke, wiec kazde pole mapujemy defensywnie.
                    return new GoogleBookResultDto
                    {
                        GoogleBooksId = item.Id ?? string.Empty,
                        Title = volume.Title ?? "Bez tytulu",
                        Author = MapAuthors(volume.Authors),
                        Description = volume.Description,
                        CoverUrl = volume.ImageLinks?.Thumbnail,
                        Isbn = MapIsbn(volume.IndustryIdentifiers)
                    };
                })
                .Where(book => !string.IsNullOrWhiteSpace(book.GoogleBooksId))
                .ToList();
        }

        private static string MapAuthors(List<string>? authors)
        {
            if (authors == null || authors.Count == 0)
                return "Nieznany autor";

            return string.Join(", ", authors);
        }

        private static string? MapIsbn(List<GoogleIndustryIdentifier>? identifiers)
        {
            if (identifiers == null || identifiers.Count == 0)
                return null;

            // Preferujemy ISBN_13, a jesli go nie ma, bierzemy pierwszy dostepny identyfikator.
            return identifiers.FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier
                ?? identifiers.FirstOrDefault()?.Identifier;
        }

        private class GoogleBooksResponse
        {
            [JsonPropertyName("items")]
            public List<GoogleBookItem>? Items { get; set; }
        }

        private class GoogleBookItem
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("volumeInfo")]
            public GoogleVolumeInfo? VolumeInfo { get; set; }
        }

        private class GoogleVolumeInfo
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("authors")]
            public List<string>? Authors { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("imageLinks")]
            public GoogleImageLinks? ImageLinks { get; set; }

            [JsonPropertyName("industryIdentifiers")]
            public List<GoogleIndustryIdentifier>? IndustryIdentifiers { get; set; }
        }

        private class GoogleImageLinks
        {
            [JsonPropertyName("thumbnail")]
            public string? Thumbnail { get; set; }
        }

        private class GoogleIndustryIdentifier
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("identifier")]
            public string? Identifier { get; set; }
        }
    }
}
