using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public byte[]? AvatarImageData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nawigacja — relacje z innymi tabelami
        public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
        public ICollection<TransactionRequest> TransactionRequests { get; set; } = new List<TransactionRequest>();
    }
}