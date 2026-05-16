using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookshelf.API.Models.Entities
{
    public class TransactionRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RequesterId { get; set; }

        [Required]
        public Guid UserBookId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Type { get; set; } = string.Empty; // loan, swap, sale

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, accepted, done, rejected

        [MaxLength(500)]
        public string? Message { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Nawigacja
        [ForeignKey("RequesterId")]
        public User Requester { get; set; } = null!;

        [ForeignKey("UserBookId")]
        public UserBook UserBook { get; set; } = null!;
    }
}