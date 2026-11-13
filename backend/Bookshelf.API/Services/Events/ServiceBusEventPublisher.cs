using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bookshelf.API.Models.Events;

namespace Bookshelf.API.Services.Events
{
    // Service Bus: zdarzenie do topicu transaction-events. Każda subskrypcja (history, notifications)
    // dostaje własną kopię, więc awaria jednego odbiorcy nie blokuje drugiego.
    public class ServiceBusEventPublisher(ServiceBusSender sender) : IEventPublisher
    {
        public Task PublishAsync(TransactionEvent evt, CancellationToken ct = default) =>
            sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(evt))
            {
                ContentType = "application/json",
                MessageId = evt.EventId.ToString() // ten sam format i id co w kolejce Storage
            }, ct);
    }
}
