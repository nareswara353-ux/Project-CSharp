using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Product : Entity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ProductSku Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product()
    {
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Product(
        string name,
        string description,
        ProductSku sku,
        Money price,
        int stockQuantity = 0)
        : base()
    {
        SetName(name);
        SetDescription(description);
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        SetStockQuantity(stockQuantity);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        SetName(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        SetDescription(description);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int newQuantity)
    {
        SetStockQuantity(newQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be positive", nameof(quantity));

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to remove must be positive", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));

        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        Description = (description ?? string.Empty).Trim();
    }

    private void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        StockQuantity = quantity;
    }
}
