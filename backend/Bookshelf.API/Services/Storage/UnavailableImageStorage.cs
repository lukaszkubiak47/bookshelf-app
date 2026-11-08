using Bookshelf.API.Exceptions;

namespace Bookshelf.API.Services.Storage
{
    // Zamiennik, gdy brak ConnectionStrings:Storage: API startuje i działa, tylko bez wgrywania obrazów.
    public class UnavailableImageStorage : IImageStorage
    {
        public Task<string> UploadAsync(string container, string prefix, IFormFile file) =>
            throw new BadRequestException("Wgrywanie obrazów jest niedostępne: brak konfiguracji Storage");

        public Task DeleteAsync(string container, string? blobName) => Task.CompletedTask;

        public Uri GetReadUri(string container, string blobName) =>
            throw new NotFoundException("Obraz niedostępny: brak konfiguracji Storage");
    }
}