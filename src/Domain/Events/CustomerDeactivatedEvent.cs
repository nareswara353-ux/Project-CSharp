namespace Domain.Events;

public sealed class CustomerDeactivatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }

    public CustomerDeactivatedEvent(Guid customerId, string email)
    {
        CustomerId = customerId;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }
}
