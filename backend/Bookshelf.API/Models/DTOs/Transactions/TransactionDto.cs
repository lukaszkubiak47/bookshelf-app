namespace Bookshelf.API.Models.DTOs.Transactions
{
    /// <summary>
    /// DTO zwracane przez endpointy transakcji. Nie wystawiamy encji EF bezposrednio.
    /// </summary>
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Skrot danych oferty, ktorej dotyczy transakcja.
        public UserBookSummaryDto UserBook { get; set; } = null!;

        // Skrot danych uzytkownika, ktory wyslal prosbe.
        public UserSummaryDto Requester { get; set; } = null!;
    }

    public class UserBookSummaryDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal? Price { get; set; }

        // Dane ksiazki sa zagniezdzone, zeby frontend nie musial skladac ich z wielu endpointow.
        public BookSummaryDto Book { get; set; } = null!;

        // Wlasciciel oferty, bez danych wrazliwych.
        public UserSummaryDto Owner { get; set; } = null!;
    }

    public class BookSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
    }

    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
