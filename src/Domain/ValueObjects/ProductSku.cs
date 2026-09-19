using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.ValueObjects;

public sealed partial class ProductSku : ValueObject
{
    public string Value { get; } = null!;

    private ProductSku() { } // For EF Core

    private ProductSku(string value)
    {
        Value = value;
    }

    public static ProductSku Create(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        var normalized = sku.Trim().ToUpperInvariant();

        if (normalized.Length < 3 || normalized.Length > 50)
            throw new ArgumentException("SKU must be between 3 and 50 characters", nameof(sku));

        if (!SkuRegex().IsMatch(normalized))
            throw new ArgumentException(
                "SKU must contain only uppercase letters, digits, and dashes (e.g., PRD-001)",
                nameof(sku));

        return new ProductSku(normalized);
    }

    public static implicit operator string(ProductSku sku) => sku.Value;

    public static explicit operator ProductSku(string value) => Create(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z0-9]+(-[A-Z0-9]+)*$")]
    private static partial Regex SkuRegex();
}
