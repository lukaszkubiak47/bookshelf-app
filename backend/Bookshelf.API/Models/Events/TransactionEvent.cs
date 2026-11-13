namespace Bookshelf.API.Models.Events
{
    // Zdarzenie zmiany statusu prośby o transakcję, wysyłane do topicu Service Bus "transaction-events".
    // Format jest kontraktem z Functions (historia) i z T3 (powiadomienia), więc zmieniaj go ostrożnie.
    public record TransactionEvent(
        Guid EventId,          // unikalny identyfikator zdarzenia (ochrona przed duplikatem w historii)
        Guid TransactionId,    // id TransactionRequest
        string Type,           // Requested | Accepted | AutoRejectedOnAccept | Rejected | Finished | AutoRejectedOnFinish
        Guid ActorUserId,      // kto wykonał akcję
        Guid RecipientUserId,  // kogo dotyczy (w T3 dostanie powiadomienie)
        Guid UserBookId,       // oferta, której dotyczy transakcja
        string? BookTitle,     // tytuł do treści powiadomienia
        DateTime OccurredAt);  // kiedy (UTC)
}