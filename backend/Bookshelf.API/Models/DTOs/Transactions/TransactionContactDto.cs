namespace Bookshelf.API.Models.DTOs.Transactions
{
    /// <summary>
    /// Dane kontaktowe DRUGIEJ strony zaakceptowanej/zakończonej transakcji.
    /// Wystawiane wyłącznie stronie transakcji i tylko po akceptacji (patrz TransactionService.GetContactAsync).
    /// </summary>
    public class TransactionContactDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
