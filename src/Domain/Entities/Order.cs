using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order : Entity
{
    private readonly List<OrderLine> _lines = new();

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order()
    {
        Status = OrderStatus.Draft;
        OrderDate = DateTime.UtcNow;
    }

    public Order(Guid customerId, string? notes = null) : base()
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        CustomerId = customerId;
        Status = OrderStatus.Draft;
        OrderDate = DateTime.UtcNow;
        Notes = notes?.Trim();
    }

    public void AddLine(Guid productId, string productName, Money unitPrice, Quantity quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to an order with status '{Status}'. Only Draft orders can be modified.");

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (unitPrice is null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (quantity is null || quantity.IsZero)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
        {
            existingLine.IncreaseQuantity(quantity);
            return;
        }

        _lines.Add(new OrderLine(productId, productName, unitPrice, quantity));
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot remove lines from an order with status '{Status}'.");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            throw new InvalidOperationException($"Order line with ID {lineId} not found.");

        _lines.Remove(line);
    }

    public Money GetTotalAmount()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot calculate total for an empty order.");

        var currency = _lines.First().UnitPrice.Currency;
        var total = new Money(0, currency);

        foreach (var line in _lines)
        {
            if (line.UnitPrice.Currency != currency)
                throw new InvalidOperationException(
                    $"Cannot mix currencies in an order: {currency} and {line.UnitPrice.Currency}");

            total = total.Add(line.GetSubtotal());
        }

        return total;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Only Draft orders can be confirmed. Current status: {Status}");

        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order with no line items.");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Only Confirmed orders can be shipped. Current status: {Status}");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel an order that has already been shipped.");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    public int GetTotalItemCount() => _lines.Sum(l => l.Quantity.Value);
}
