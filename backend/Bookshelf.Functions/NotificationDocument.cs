using Microsoft.Azure.Cosmos;

namespace Bookshelf.Functions
{
	// Dokument powiadomienia w Cosmos DB (kontener notifications, partycja /userId).
	// Kopia w Bookshelf.API/Services/Notifications/NotificationDocument.cs: nazwy pól muszą się zgadzać.
	public record NotificationDocument(
		string id,            // = EventId: ponowione zdarzenie nie zrobi duplikatu
		string userId,        // = RecipientUserId, klucz partycji
		string type,
		string transactionId,
		string? bookTitle,
		DateTime createdAt,
		bool isRead);

	// Kontener powiadomień; Container == null, gdy brak ustawienia CosmosConnection (funkcja pisze wtedy tylko historię).
	public record NotificationsContainer(Container? Container);
}
