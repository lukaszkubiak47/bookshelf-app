using System.Text.Json;
using Azure.Storage.Queues;

namespace Bookshelf.API.Services.Storage
{
    // Zadanie dla funkcji GenerateThumbnail. JSON: { "container": "covers", "blobName": "..." }
    public record ImageJob(string Container, string BlobName);

    public interface IImageJobQueue
    {
        Task EnqueueAsync(ImageJob job);
    }

    // Kolejka image-jobs (Base64, jak transaction-events). Błąd kolejki nie psuje wgrania okładki: oryginał i tak działa.
    public class QueueImageJobQueue(QueueClient queue, ILogger<QueueImageJobQueue> logger) : IImageJobQueue
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web); // camelCase

        public async Task EnqueueAsync(ImageJob job)
        {
            try { await queue.SendMessageAsync(JsonSerializer.Serialize(job, Json)); }
            catch (Exception ex) { logger.LogWarning(ex, "Nie udało się wysłać zadania miniatury dla {Blob}", job.BlobName); }
        }
    }

    // Brak Storage: nic nie robimy (i tak nie da się wgrać okładki).
    public class NoImageJobQueue : IImageJobQueue
    {
        public Task EnqueueAsync(ImageJob job) => Task.CompletedTask;
    }
}
