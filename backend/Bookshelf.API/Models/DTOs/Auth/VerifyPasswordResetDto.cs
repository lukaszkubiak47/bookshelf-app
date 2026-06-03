using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Auth
{
    public class VerifyPasswordResetDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(3, ErrorMessage = "Nick musi miec co najmniej 3 znaki")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "Miesiac zalozenia konta musi byc w zakresie 1-12")]
        public int CreatedMonth { get; set; }

        [MaxLength(100)]
        public string? FounderAnswer { get; set; }
    }
}
