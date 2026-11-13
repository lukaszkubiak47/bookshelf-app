using Bookshelf.API.Exceptions;
using Bookshelf.API.Models.DTOs.Notifications;

namespace Bookshelf.API.Services.Notifications
{
    // Zamiennik, gdy brak ConnectionStrings:Cosmos: API działa, dzwonek pokazuje pustą listę.
    public class NoNotificationService : INotificationService
    {
        public Task<NotificationListDto> GetForUserAsync(Guid userId) =>
            Task.FromResult(new NotificationListDto());

        public Task MarkReadAsync(Guid userId, string notificationId) =>
            throw new NotFoundException("Powiadomienie nie istnieje");
    }
}
