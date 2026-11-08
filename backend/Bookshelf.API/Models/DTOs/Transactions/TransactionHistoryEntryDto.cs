namespace Bookshelf.API.Models.DTOs.Transactions
{
    /// <summary>
    /// Wpis osi czasu transakcji (z tabeli TransactionHistory, zapisanej przez funkcję).
    /// </summary>
    public class TransactionHistoryEntryDto
    {
        public string Type { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public Guid ActorUserId { get; set; }
    }
}