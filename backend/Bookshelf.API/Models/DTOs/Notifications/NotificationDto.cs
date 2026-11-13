namespace Bookshelf.API.Models.DTOs.Notifications
{
    /// <summary>
    /// Powiadomienie o akcji na transakcji (z Cosmos DB, zapisane przez funkcję).
    /// </summary>
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
        public string? BookTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    /// <summary>
    /// Najnowsze powiadomienia użytkownika i liczba nieprzeczytanych (do licznika przy dzwonku).
    /// </summary>
    public class NotificationListDto
    {
        public List<NotificationDto> Items { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
