using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class OrderCreatedEventHandler
    : INotificationHandler<DomainEventNotification<OrderCreatedEvent>>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<OrderCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId}: {Amount} {Currency}",
            evt.OrderId,
            evt.CustomerId,
            evt.TotalAmount,
            evt.Currency);

        return Task.CompletedTask;
    }
}
