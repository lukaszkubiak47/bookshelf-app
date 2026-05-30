namespace Bookshelf.API.Models.DTOs.Books
{
    /// <summary>
    /// Ksiazka zapisana w naszej bazie, zwracana do frontendu.
    /// </summary>
    public class BookDto
    {
        public Guid Id { get; set; }
        public string? GoogleBooksId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Isbn { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
