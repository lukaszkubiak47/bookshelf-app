using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Books
{
    public class CreateBookDto
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Author { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ISBN { get; set; }

        [MaxLength(100)]
        public string? GenreName { get; set; }
    }
}
