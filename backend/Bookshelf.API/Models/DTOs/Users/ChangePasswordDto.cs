using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Users
{
    /// <summary>
    /// Request do zmiany hasla. CurrentPassword weryfikujemy, a NewPassword zapisujemy po zahashowaniu.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Nowe haslo musi miec co najmniej 8 znakow")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
