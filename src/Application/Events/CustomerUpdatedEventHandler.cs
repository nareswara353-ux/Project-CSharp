using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class CustomerUpdatedEventHandler
    : INotificationHandler<DomainEventNotification<CustomerUpdatedEvent>>
{
    private readonly ILogger<CustomerUpdatedEventHandler> _logger;

    public CustomerUpdatedEventHandler(ILogger<CustomerUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<CustomerUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "Customer {CustomerId} updated: {FullName} <{Email}>",
            evt.CustomerId,
            evt.FullName,
            evt.Email);

        return Task.CompletedTask;
    }
}
