namespace Bookshelf.API.Models.DTOs.Users
{
    /// <summary>
    /// Dane użytkownika zwracane do frontendu. Nie zawiera PasswordHash ani innych pól wrażliwych.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Liczniki do widoku profilu / "Moja polka".
        public int BooksCount { get; set; }
        public int TransactionsCount { get; set; }
    }
}
