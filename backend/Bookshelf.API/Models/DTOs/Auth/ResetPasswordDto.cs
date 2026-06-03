using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public VerifyPasswordResetDto Verification { get; set; } = new();

        [Required]
        [MinLength(8, ErrorMessage = "Nowe haslo musi miec co najmniej 8 znakow")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
