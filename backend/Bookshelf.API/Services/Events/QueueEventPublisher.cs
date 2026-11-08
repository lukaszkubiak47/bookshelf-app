using System.Text.Json;
using Azure.Storage.Queues;
using Bookshelf.API.Models.Events;

namespace Bookshelf.API.Services.Events
{
    // Azure Storage albo Azurite: zdarzenie jako komunikat JSON w kolejce transaction-events.
    public class QueueEventPublisher(QueueClient queue) : IEventPublisher
    {
        public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default) =>
            queue.SendMessageAsync(JsonSerializer.Serialize(evt), ct);
    }
}