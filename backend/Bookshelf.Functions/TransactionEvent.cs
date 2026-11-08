namespace Bookshelf.Functions
{
	// Kopia Bookshelf.API/Models/Events/TransactionEvent.cs: nazwy pól muszą się zgadzać, bo to format JSON z kolejki.
	public record TransactionEvent(
		Guid EventId,
		Guid TransactionId,
		string Type,
		Guid ActorUserId,
		Guid RecipientUserId,
		Guid UserBookId,
		string? BookTitle,
		DateTime OccurredAt);
}