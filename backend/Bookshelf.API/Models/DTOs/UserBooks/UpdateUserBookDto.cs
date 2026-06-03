using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.UserBooks
{
    public class UpdateUserBookDto
    {
        [RegularExpression("^(loan|swap|sale)$", ErrorMessage = "TransactionType musi być: loan, swap lub sale")]
        public string? TransactionType { get; set; }

        [Range(0, 99999)]
        public decimal? Price { get; set; }

        [RegularExpression("^(specific_book|genre|anything|free)$")]
        public string? ExchangeCondition { get; set; }

        [MaxLength(300)]
        public string? ExchangeDetails { get; set; }

        [MaxLength(2000)]
        public string? BookDescription { get; set; }
    }
}