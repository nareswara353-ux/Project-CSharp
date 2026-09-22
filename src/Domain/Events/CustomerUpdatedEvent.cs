namespace Domain.Events;

public sealed class CustomerUpdatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public string FullName { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }

    public CustomerUpdatedEvent(Guid customerId, string fullName, string email)
    {
        CustomerId = customerId;
        FullName = fullName;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }
}
