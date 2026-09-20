namespace Domain.Events;

public sealed class ProductStockLowEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }
    public int Threshold { get; }
    public DateTime OccurredOn { get; }

    public ProductStockLowEvent(Guid productId, string productName, int currentStock, int threshold)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        Threshold = threshold;
        OccurredOn = DateTime.UtcNow;
    }
}
