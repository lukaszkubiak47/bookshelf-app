using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.UserBooks
{
    public class CreateUserBookDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        [RegularExpression("^(loan|swap|sale)$", ErrorMessage = "TransactionType musi być: loan, swap lub sale")]
        public string TransactionType { get; set; } = string.Empty;

        [Range(0, 99999, ErrorMessage = "Cena musi być w zakresie 0-99999")]
        public decimal? Price { get; set; }

        [RegularExpression("^(specific_book|genre|anything|free)$", ErrorMessage = "ExchangeCondition musi być: specific_book, genre, anything lub free")]
        public string? ExchangeCondition { get; set; }

        [MaxLength(300)]
        public string? ExchangeDetails { get; set; }
    }
}