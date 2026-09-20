using Domain.Events;

namespace Domain.Common;

public static class DomainEventExtensions
{
    public static bool HasDomainEvents(this IHasDomainEvents aggregate)
        => aggregate.DomainEvents.Count > 0;

    public static IEnumerable<TEvent> GetDomainEvents<TEvent>(this IHasDomainEvents aggregate)
        where TEvent : IDomainEvent
        => aggregate.DomainEvents.OfType<TEvent>();

    public static bool HasDomainEvent<TEvent>(this IHasDomainEvents aggregate)
        where TEvent : IDomainEvent
        => aggregate.DomainEvents.OfType<TEvent>().Any();
}
