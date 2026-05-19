using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Users
{
    /// <summary>
    /// Dane, ktore uzytkownik moze edytowac w swoim profilu. Wszystkie pola sa opcjonalne.
    /// </summary>
    public class UpdateUserDto
    {
        [MinLength(3, ErrorMessage = "Nick musi miec co najmniej 3 znaki")]
        [MaxLength(50)]
        public string? Username { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }
    }
}
