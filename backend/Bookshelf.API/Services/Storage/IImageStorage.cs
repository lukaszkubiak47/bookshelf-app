namespace Bookshelf.API.Services.Storage
{
    // Obrazy (avatary, okładki) w Blob Storage. W bazie trzymamy tylko nazwę pliku (blobName).
    public interface IImageStorage
    {
        // Zapisuje plik w kontenerze i zwraca nazwę bloba, np. "a1b2.../3f4e....png".
        Task<string> UploadAsync(string container, string prefix, IFormFile file);

        // Usuwa plik; brak nazwy lub brak pliku nie jest błędem.
        Task DeleteAsync(string container, string? blobName);

        // Krótki link SAS (tylko odczyt, 15 min) do wydania obrazu przeglądarce.
        Uri GetReadUri(string container, string blobName);

        // Czy plik istnieje (np. czy funkcja zdążyła już zrobić miniaturę).
        Task<bool> ExistsAsync(string container, string blobName);
    }
}