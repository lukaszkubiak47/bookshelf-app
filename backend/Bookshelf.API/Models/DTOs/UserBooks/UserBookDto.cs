namespace Bookshelf.API.Models.DTOs.UserBooks
{
    public class UserBookDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? ExchangeCondition { get; set; }
        public string? ExchangeDetails { get; set; }
        public bool HasCustomCover { get; set; }
        public DateTime CreatedAt { get; set; }

        // Dane książki (zagnieżdżone)
        public BookInfoDto Book { get; set; } = null!;

        // Dane właściciela (zagnieżdżone, BEZ email!)
        public OwnerInfoDto Owner { get; set; } = null!;
    }

    public class BookInfoDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public string? ISBN { get; set; }
        public string? GenreName { get; set; }
    }

    public class OwnerInfoDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}