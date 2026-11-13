using System.Text.Json;
using Microsoft.Azure.Functions.Worker;

namespace Bookshelf.Functions
{
    // Service Bus: topic transaction-events, dwie subskrypcje. Każda funkcja dostaje własną kopię zdarzenia,
    // więc błąd zapisu powiadomienia nie blokuje historii (i odwrotnie). Nieudany komunikat Service Bus
    // ponawia sam, a po 10 próbach odkłada do dead-letter danej subskrypcji.
    public class TransactionEventSubscribers(TransactionEventStore store)
    {
        [Function("ProcessTransactionHistory")]
        public Task History(
            [ServiceBusTrigger("transaction-events", "history", Connection = "ServiceBusConnection")] string message) =>
            store.WriteHistoryAsync(Parse(message));

        [Function("ProcessTransactionNotification")]
        public Task Notify(
            [ServiceBusTrigger("transaction-events", "notifications", Connection = "ServiceBusConnection")] string message) =>
            store.WriteNotificationAsync(Parse(message));

        // Ten sam JSON co w kolejce Storage (bez Base64)
        private static TransactionEvent Parse(string message) =>
            JsonSerializer.Deserialize<TransactionEvent>(message)!;
    }
}
