using System.Text.Json;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class DomainEventLoggingHandler<TEvent>
    : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent
{
    private readonly ILogger<DomainEventLoggingHandler<TEvent>> _logger;

    public DomainEventLoggingHandler(ILogger<DomainEventLoggingHandler<TEvent>> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<TEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogDebug(
            "Domain event {EventType} occurred at {OccurredOn}: {Payload}",
            typeof(TEvent).Name,
            evt.OccurredOn,
            JsonSerializer.Serialize(evt));

        return Task.CompletedTask;
    }
}
