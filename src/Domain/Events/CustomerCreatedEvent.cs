namespace Domain.Events;

public sealed class CustomerCreatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public string Email { get; }
    public string FullName { get; }
    public DateTime OccurredOn { get; }

    public CustomerCreatedEvent(Guid customerId, string email, string fullName)
    {
        CustomerId = customerId;
        Email = email;
        FullName = fullName;
        OccurredOn = DateTime.UtcNow;
    }
}
