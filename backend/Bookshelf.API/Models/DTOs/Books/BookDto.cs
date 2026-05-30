namespace Bookshelf.API.Models.DTOs.Books
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public string? ISBN { get; set; }
        public string? GenreName { get; set; }
    }
}
