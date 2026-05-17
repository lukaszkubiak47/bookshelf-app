using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(3, ErrorMessage = "Nick musi mieć co najmniej 3 znaki")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }
}