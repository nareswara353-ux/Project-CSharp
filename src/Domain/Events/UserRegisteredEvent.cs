namespace Domain.Events;

public sealed class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Username { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }

    public UserRegisteredEvent(Guid userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }
}
