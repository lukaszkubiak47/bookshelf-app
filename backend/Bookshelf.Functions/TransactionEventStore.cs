using Azure.Data.Tables;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Bookshelf.Functions
{
    // Zapis skutków zdarzenia transakcji: historia (Table Storage) i powiadomienie (Cosmos DB).
    // Używane przez funkcje na Service Bus (TransactionEventSubscribers).
    public class TransactionEventStore(
        TableClient history,
        NotificationsContainer notifications,
        ILogger<TransactionEventStore> logger)
    {
        public async Task WriteHistoryAsync(TransactionEvent evt)
        {
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

        public async Task WriteNotificationAsync(TransactionEvent evt)
        {
            if (notifications.Container is null)
                return;

            // id = EventId: jeśli zdarzenie przyjdzie ponownie, Create zwróci 409 i nie powstanie duplikat.
            // Create zamiast Upsert, żeby ponowienie nie cofnęło "przeczytane" na "nieprzeczytane".
            var doc = new NotificationDocument(
                evt.EventId.ToString(),
                evt.RecipientUserId.ToString(),
                evt.Type,
                evt.TransactionId.ToString(),
                evt.BookTitle,
                DateTime.SpecifyKind(evt.OccurredAt, DateTimeKind.Utc),
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
