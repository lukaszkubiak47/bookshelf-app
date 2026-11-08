using Bookshelf.API.Models.Events;

namespace Bookshelf.API.Services.Events
{
    // Wysyłanie zdarzeń "w tło": API wrzuca zdarzenie i od razu odpowiada, resztę robi funkcja.
    public interface IEventPublisher
    {
        Task PublishAsync(TransactionEvent evt, CancellationToken ct = default);
    }
}