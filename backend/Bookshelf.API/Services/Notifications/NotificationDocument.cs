namespace Bookshelf.API.Services.Notifications
{
    // Dokument powiadomienia w Cosmos DB (kontener notifications, partycja /userId), zapisywany przez funkcję ProcessTransactionEvent.
    // Kopia Bookshelf.Functions/NotificationDocument.cs: nazwy pól muszą się zgadzać.
    public record NotificationDocument(
        string id,
        string userId,
        string type,
        string transactionId,
        string? bookTitle,
        DateTime createdAt,
        bool isRead);
}
