using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Transactions
{
    /// <summary>
    /// Request do wyslania prosby o ksiazke. Typ transakcji bierzemy z oferty UserBook.
    /// </summary>
    public class CreateTransactionDto
    {
        [Required]
        public Guid UserBookId { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        // Proponowana data zwrotu. Ma sens glownie dla wypozyczenia (loan).
        public DateTime? DueDate { get; set; }
    }
}
