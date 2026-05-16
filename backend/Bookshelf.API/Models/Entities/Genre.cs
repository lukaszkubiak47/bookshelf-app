using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.Entities
{
    public class Genre
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Nawigacja — książki w tym gatunku
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}