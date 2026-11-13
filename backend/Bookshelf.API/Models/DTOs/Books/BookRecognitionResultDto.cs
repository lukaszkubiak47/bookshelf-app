namespace Bookshelf.API.Models.DTOs.Books
{
    /// <summary>
    /// Wynik rozpoznania okładki: tekst odczytany ze zdjęcia (użyty jako zapytanie) i wyniki Google Books.
    /// </summary>
    public class BookRecognitionResultDto
    {
        public string Query { get; set; } = string.Empty;
        public List<GoogleBookResultDto> Results { get; set; } = new();
    }
}
