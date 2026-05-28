using Bookshelf.API.Models.DTOs.Transactions;

namespace Bookshelf.API.Services
{
    public interface ITransactionService
    {
        // Wysyla prosbe o ksiazke dla wybranej oferty UserBook.
        Task<TransactionDto> CreateAsync(Guid requesterId, CreateTransactionDto dto);

        // Pobiera prosby otrzymane przez wlasciciela ksiazek.
        Task<List<TransactionDto>> GetReceivedAsync(Guid ownerId);

        // Pobiera prosby wyslane przez zalogowanego uzytkownika.
        Task<List<TransactionDto>> GetSentAsync(Guid requesterId);

        // Akceptuje prosbe i rezerwuje / wypozycza oferte.
        Task<TransactionDto> AcceptAsync(Guid ownerId, Guid transactionId, AcceptTransactionDto dto);

        // Odrzuca prosbe bez zmiany dostepnosci oferty.
        Task<TransactionDto> RejectAsync(Guid ownerId, Guid transactionId);

        // Konczy zaakceptowana transakcje i przywraca oferte jako dostepna.
        Task<TransactionDto> FinishAsync(Guid ownerId, Guid transactionId);
    }
}
