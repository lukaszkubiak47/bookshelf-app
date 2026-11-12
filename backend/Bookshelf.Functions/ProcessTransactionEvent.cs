using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bookshelf.Functions
{
    // Odbiera zdarzenia transakcji z kolejki: zapisuje historię w tabeli TransactionHistory
    // i powiadomienie dla drugiej strony w Cosmos DB.
    // Jedna funkcja na kolejkę: dwie funkcje na tej samej kolejce dzieliłyby się komunikatami.
    public class ProcessTransactionEvent(
        TableClient history,
        NotificationsContainer notifications,
        ILogger<ProcessTransactionEvent> logger)
    {
        [Function(nameof(ProcessTransactionEvent))]
        public async Task Run(
            [QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
        {
            var evt = JsonSerializer.Deserialize<TransactionEvent>(message)!;
            var occurredAt = DateTime.SpecifyKind(evt.OccurredAt, DateTimeKind.Utc);

            // PartitionKey = transakcja (jedno zapytanie zwraca całą historię),
            // RowKey zaczyna się od czasu -> sortowanie chronologiczne; EventId chroni przed duplikatem
            var entity = new TableEntity(evt.TransactionId.ToString(), $"{evt.OccurredAt:yyyyMMddHHmmssfff}-{evt.EventId}")
            {
                ["Type"] = evt.Type,
                ["ActorUserId"] = evt.ActorUserId.ToString(),
                ["OccurredAt"] = occurredAt
            };

            await history.UpsertEntityAsync(entity); // upsert: ponowione zdarzenie nie zrobi duplikatu
            logger.LogInformation("Historia: {Type} dla {Id}", evt.Type, evt.TransactionId);

            if (notifications.Container is null)
                return;

            // id = EventId: jeśli kolejka ponowi komunikat, Create zwróci 409 i nie powstanie duplikat.
            // Create zamiast Upsert, żeby ponowienie nie cofnęło "przeczytane" na "nieprzeczytane".
            var doc = new NotificationDocument(
                evt.EventId.ToString(),
                evt.RecipientUserId.ToString(),
                evt.Type,
                evt.TransactionId.ToString(),
                evt.BookTitle,
                occurredAt,
                false);

            try
            {
                await notifications.Container.CreateItemAsync(doc, new PartitionKey(doc.userId));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                logger.LogInformation("Powiadomienie {Id} już istnieje (ponowione zdarzenie)", doc.id);
                return;
            }
            logger.LogInformation("Powiadomienie: {Type} dla użytkownika {UserId}", evt.Type, doc.userId);
        }
    }
}
