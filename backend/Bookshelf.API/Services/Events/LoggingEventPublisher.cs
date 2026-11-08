using Bookshelf.API.Models.Events;

namespace Bookshelf.API.Services.Events
{
    // Zamiennik bez Storage: tylko wpis w logu, żeby API działało bez Azure i bez Azurite.
    public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
    {
        public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default)
        {
            logger.LogInformation("Zdarzenie {Type} transakcji {Id}", evt.Type, evt.TransactionId);
            return Task.CompletedTask;
        }
    }
}