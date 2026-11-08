using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Bookshelf.API.Services.Storage
{
    public class BlobImageStorage(BlobServiceClient blobService, IConfiguration configuration) : IImageStorage
    {
        // Lokalnie w dockerze: adres bloba widoczny z przeglądarki (http://localhost:10000/...).
        // W Azure puste — linki *.blob.core.windows.net działają bez podmiany.
        private readonly string? _publicBlobUrl = configuration["Storage:PublicBlobUrl"];

        public async Task<string> UploadAsync(string container, string prefix, IFormFile file)
        {
            // Rozszerzenie z typu pliku (walidacja JPG/PNG jest w serwisach przed wywołaniem).
            var extension = file.ContentType == "image/png" ? ".png" : ".jpg";
            var blobName = $"{prefix}/{Guid.NewGuid()}{extension}";

            var blob = blobService.GetBlobContainerClient(container).GetBlobClient(blobName);
            await using var stream = file.OpenReadStream();

            // Typ pliku zapisany w nagłówku bloba, więc w bazie nie potrzeba kolumny na typ.
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });

            return blobName;
        }

        public async Task DeleteAsync(string container, string? blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName)) return;
            await blobService.GetBlobContainerClient(container).DeleteBlobIfExistsAsync(blobName);
        }

        public Uri GetReadUri(string container, string blobName)
        {
            var blob = blobService.GetBlobContainerClient(container).GetBlobClient(blobName);

            // Działa, bo connection string zawiera klucz konta (podpis SAS liczony lokalnie, bez zapytania do Azure).
            var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15));

            if (string.IsNullOrWhiteSpace(_publicBlobUrl)) return sasUri;

            // Azurite w dockerze: http://azurite:10000/devstoreaccount1/... -> http://localhost:10000/devstoreaccount1/...
            var internalBase = blobService.Uri.ToString().TrimEnd('/');
            var url = sasUri.ToString();
            return url.StartsWith(internalBase, StringComparison.OrdinalIgnoreCase)
                ? new Uri(_publicBlobUrl.TrimEnd('/') + url[internalBase.Length..])
                : sasUri;
        }

        public async Task<bool> ExistsAsync(string container, string blobName) =>
            (await blobService.GetBlobContainerClient(container).GetBlobClient(blobName).ExistsAsync()).Value;
    }
}