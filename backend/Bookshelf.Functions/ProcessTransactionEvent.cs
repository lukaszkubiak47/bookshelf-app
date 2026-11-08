using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bookshelf.Functions
{
    // Odbiera zdarzenia transakcji z kolejki. Na razie tylko loguje, w kroku 4 zapisze historię.
    public class ProcessTransactionEvent(ILogger<ProcessTransactionEvent> logger)
    {
        [Function(nameof(ProcessTransactionEvent))]
        public void Run(
            [QueueTrigger("transaction-events", Connection = "StorageConnection")] string message)
        {
            logger.LogInformation("Zdarzenie z kolejki: {Message}", message);
        }
    }
}