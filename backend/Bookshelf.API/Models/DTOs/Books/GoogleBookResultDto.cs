namespace Bookshelf.API.Models.DTOs.Books
{
    /// <summary>
    /// Pojedynczy wynik wyszukiwania z Google Books API.
    /// </summary>
    public class GoogleBookResultDto
    {
        public string GoogleBooksId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public string? Isbn { get; set; }
    }
}
