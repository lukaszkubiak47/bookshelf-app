using System.ComponentModel.DataAnnotations;

namespace Bookshelf.API.Models.DTOs.Transactions
{
    /// <summary>
    /// Request do zaakceptowania prosby. Wlasciciel moze doprecyzowac date i wiadomosc.
    /// </summary>
    public class AcceptTransactionDto
    {
        // Wlasciciel moze potwierdzic albo zmienic proponowana date zwrotu.
        public DateTime? DueDate { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
