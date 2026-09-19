using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class OrderLine : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;

    private OrderLine()
    {
        ProductName = null!;
        UnitPrice = null!;
        Quantity = null!;
    }

    internal OrderLine(Guid productId, string productName, Money unitPrice, Quantity quantity) : base()
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
    }

    public Money GetSubtotal()
    {
        return UnitPrice.Multiply(Quantity.Value);
    }

    internal void IncreaseQuantity(Quantity additionalQuantity)
    {
        if (additionalQuantity is null || additionalQuantity.IsZero)
            throw new ArgumentException("Additional quantity must be positive", nameof(additionalQuantity));

        Quantity = Quantity.Add(additionalQuantity);
    }

    internal void UpdateQuantity(Quantity newQuantity)
    {
        if (newQuantity is null || newQuantity.IsZero)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
    }
}
