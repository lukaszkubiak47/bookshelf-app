using System.Net;
using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Notifications;
using Microsoft.Azure.Cosmos;

namespace Bookshelf.API.Services.Notifications
{
    // Powiadomienia z Cosmos DB. Każde zapytanie idzie do partycji zalogowanego użytkownika,
    // więc nikt nie odczyta ani nie oznaczy cudzego powiadomienia.
    public class CosmosNotificationService : INotificationService
    {
        private const int PageSize = 20;
        private readonly Container _container;

        public CosmosNotificationService(Container container)
        {
            _container = container;
        }

        public async Task<NotificationListDto> GetForUserAsync(Guid userId)
        {
            var partition = new QueryRequestOptions { PartitionKey = new PartitionKey(userId.ToString()) };

            var itemsQuery = new QueryDefinition(
                    $"SELECT TOP {PageSize} * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
                .WithParameter("@userId", userId.ToString());

            var items = new List<NotificationDto>();
            using (var feed = _container.GetItemQueryIterator<NotificationDocument>(itemsQuery, requestOptions: partition))
            {
                while (feed.HasMoreResults)
                {
                    foreach (var doc in await feed.ReadNextAsync())
                        items.Add(ToDto(doc));
                }
            }

            var countQuery = new QueryDefinition(
                    "SELECT VALUE COUNT(1) FROM c WHERE c.userId = @userId AND c.isRead = false")
                .WithParameter("@userId", userId.ToString());

            var unread = 0;
            using (var feed = _container.GetItemQueryIterator<int>(countQuery, requestOptions: partition))
            {
                while (feed.HasMoreResults)
                    unread += (await feed.ReadNextAsync()).Sum();
            }

            return new NotificationListDto { Items = items, UnreadCount = unread };
        }

        public async Task MarkReadAsync(Guid userId, string notificationId)
        {
            try
            {
                await _container.PatchItemAsync<NotificationDocument>(
                    notificationId,
                    new PartitionKey(userId.ToString()),
                    [PatchOperation.Set("/isRead", true)]);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Brak w partycji użytkownika: nie istnieje, wygasło (TTL) albo jest cudze.
                throw new NotFoundException("Powiadomienie nie istnieje");
            }
        }

        private static NotificationDto ToDto(NotificationDocument doc) => new()
        {
            Id = doc.id,
            Type = doc.type,
            TransactionId = Guid.TryParse(doc.transactionId, out var id) ? id : Guid.Empty,
            BookTitle = doc.bookTitle,
            CreatedAt = DateTime.SpecifyKind(doc.createdAt, DateTimeKind.Utc),
            IsRead = doc.isRead
        };
    }
}
