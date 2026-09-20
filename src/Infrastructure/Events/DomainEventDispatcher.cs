using System.Reflection;
using Application.Common;
using Domain.Common;
using Domain.Events;
using MediatR;

namespace Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(
        IEnumerable<IHasDomainEvents> entities,
        CancellationToken cancellationToken = default)
    {
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }

    private async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var notificationType = typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType());

        var notification = Activator.CreateInstance(notificationType, domainEvent)
            ?? throw new InvalidOperationException(
                $"Failed to create notification for {domainEvent.GetType().Name}.");

        await _mediator.Publish(notification, cancellationToken);
    }
}
