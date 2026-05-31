namespace Bookshelf.API.Models.DTOs.Books
{
    /// <summary>
    /// Slownikowy gatunek ksiazki, uzywany np. do dropdowna na froncie.
    /// </summary>
    public class GenreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
