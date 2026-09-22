using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class ProductStockLowEventHandler
    : INotificationHandler<DomainEventNotification<ProductStockLowEvent>>
{
    private readonly ILogger<ProductStockLowEventHandler> _logger;

    public ProductStockLowEventHandler(ILogger<ProductStockLowEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<ProductStockLowEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogWarning(
            "Low stock alert for product {ProductId} ({ProductName}): {Current}/{Threshold}",
            evt.ProductId,
            evt.ProductName,
            evt.CurrentStock,
            evt.Threshold);

        return Task.CompletedTask;
    }
}
