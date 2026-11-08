using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bookshelf.Functions
{
    // Odbiera zdarzenia transakcji z kolejki i zapisuje je w tabeli TransactionHistory.
    public class ProcessTransactionEvent(TableClient history, ILogger<ProcessTransactionEvent> logger)
    {
        [Function(nameof(ProcessTransactionEvent))]
        public async Task Run(
            [QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
        {
            var evt = JsonSerializer.Deserialize<TransactionEvent>(message)!;

            // PartitionKey = transakcja (jedno zapytanie zwraca całą historię),
            // RowKey zaczyna się od czasu -> sortowanie chronologiczne; EventId chroni przed duplikatem
            var entity = new TableEntity(evt.TransactionId.ToString(), $"{evt.OccurredAt:yyyyMMddHHmmssfff}-{evt.EventId}")
            {
                ["Type"] = evt.Type,
                ["ActorUserId"] = evt.ActorUserId.ToString(),
                ["OccurredAt"] = DateTime.SpecifyKind(evt.OccurredAt, DateTimeKind.Utc)
            };

            await history.UpsertEntityAsync(entity); // upsert: ponowione zdarzenie nie zrobi duplikatu
            logger.LogInformation("Historia: {Type} dla {Id}", evt.Type, evt.TransactionId);
        }
    }
}