using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Books
{
    /// <summary>
    /// Dane potrzebne do zapisania wybranej ksiazki w naszej bazie.
    /// </summary>
    public class CreateBookDto
    {
        [MaxLength(50)]
        public string? GoogleBooksId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        // Walidowany jawnie w serwisie (null / Guid.Empty -> BadRequestException),
        // zeby blad szedl przez nasz middleware zamiast automatycznego 400 z ModelState.
        public Guid? GenreId { get; set; }

        [MaxLength(500)]
        public string? CoverUrl { get; set; }

        [MaxLength(20)]
        public string? Isbn { get; set; }
    }
}
