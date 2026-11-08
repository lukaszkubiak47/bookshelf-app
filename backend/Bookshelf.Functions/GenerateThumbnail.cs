using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Bookshelf.Functions
{
    // Format z API: { "container": "covers", "blobName": "..." }
    public record ImageJob(string Container, string BlobName);

    // Robi miniaturę okładki (szerokość 300 px) w kontenerze thumbnails, pod tą samą nazwą.
    public class GenerateThumbnail(BlobServiceClient blobs, ILogger<GenerateThumbnail> logger)
    {
        private const int ThumbWidth = 300;
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        [Function(nameof(GenerateThumbnail))]
        public async Task Run(
            [QueueTrigger("image-jobs", Connection = "StorageConnection")] string message)
        {
            var job = JsonSerializer.Deserialize<ImageJob>(message, Json)!;
            var source = blobs.GetBlobContainerClient(job.Container).GetBlobClient(job.BlobName);

            // Okładka mogła zostać usunięta albo podmieniona, zanim funkcja ruszyła
            if (!(await source.ExistsAsync()).Value)
            {
                logger.LogWarning("Brak pliku {Container}/{Blob}, pomijam miniaturę", job.Container, job.BlobName);
                return;
            }

            var download = await source.DownloadContentAsync();
            using var image = Image.Load(download.Value.Content.ToStream());

            // Zmniejszamy tylko większe obrazy; wysokość 0 = zachowaj proporcje
            if (image.Width > ThumbWidth)
                image.Mutate(x => x.Resize(ThumbWidth, 0));

            using var output = new MemoryStream();
            await image.SaveAsync(output, image.Metadata.DecodedImageFormat!); // ten sam format co oryginał (JPG/PNG)
            output.Position = 0;

            var thumb = blobs.GetBlobContainerClient("thumbnails").GetBlobClient(job.BlobName);
            await thumb.UploadAsync(output, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = download.Value.Details.ContentType }
            }); // nadpisuje: ponowione zadanie nie zrobi błędu

            logger.LogInformation("Miniatura: {Blob} ({Width}x{Height})", job.BlobName, image.Width, image.Height);
        }
    }
}
