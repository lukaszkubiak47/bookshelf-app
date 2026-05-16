using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookshelf.API.Models.Entities
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string? GoogleBooksId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Author { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? GenreId { get; set; }

        [MaxLength(500)]
        public string? CoverUrl { get; set; }

        [MaxLength(20)]
        public string? ISBN { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nawigacja
        [ForeignKey("GenreId")]
        public Genre? Genre { get; set; }

        public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
    }
}