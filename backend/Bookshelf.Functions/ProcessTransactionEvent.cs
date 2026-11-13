using System.Text.Json;
using Microsoft.Azure.Functions.Worker;

namespace Bookshelf.Functions
{
    // STARA ŚCIEŻKA (kolejka Storage): działa, dopóki API nie ma ConnectionStrings:ServiceBus.
    // Do usunięcia osobnym PR-em, gdy historia i powiadomienia działają przez Service Bus (krok 6.4 instrukcji T3).
    public class ProcessTransactionEvent(TransactionEventStore store)
    {
        [Function(nameof(ProcessTransactionEvent))]
        public async Task Run(
            [QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
        {
            var evt = JsonSerializer.Deserialize<TransactionEvent>(message)!;
            await store.WriteHistoryAsync(evt);
            await store.WriteNotificationAsync(evt);
        }
    }
}
