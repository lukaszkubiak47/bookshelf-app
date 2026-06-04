using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookshelf.API.Models.Entities
{
    public class UserBook
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "available"; // available, borrowed, reserved

        [Required]
        [MaxLength(10)]
        public string TransactionType { get; set; } = string.Empty; // loan, swap, sale

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }

        [MaxLength(20)]
        public string? ExchangeCondition { get; set; } // specific_book, genre, anything, free

        [MaxLength(300)]
        public string? ExchangeDetails { get; set; } // np. "Szuka: Fantasy"

        [MaxLength(2000)]
        public string? Description { get; set; } // opis specyficzny dla tej oferty; null => fallback na Book.Description

        public byte[]? CustomCoverImageData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nawigacja
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;

        public ICollection<TransactionRequest> TransactionRequests { get; set; } = new List<TransactionRequest>();
    }
}