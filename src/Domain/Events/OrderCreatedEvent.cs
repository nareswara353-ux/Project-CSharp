namespace Domain.Events;

public sealed class OrderCreatedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
    public DateTime OccurredOn { get; }

    public OrderCreatedEvent(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
        OccurredOn = DateTime.UtcNow;
    }
}
