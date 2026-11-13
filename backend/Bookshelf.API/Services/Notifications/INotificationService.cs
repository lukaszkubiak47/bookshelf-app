using Bookshelf.API.Models.DTOs.Notifications;

namespace Bookshelf.API.Services.Notifications
{
    public interface INotificationService
    {
        /// <summary>Najnowsze powiadomienia użytkownika i liczba nieprzeczytanych.</summary>
        Task<NotificationListDto> GetForUserAsync(Guid userId);

        /// <summary>Oznacza powiadomienie jako przeczytane; tylko w partycji danego użytkownika.</summary>
        Task MarkReadAsync(Guid userId, string notificationId);
    }
}
