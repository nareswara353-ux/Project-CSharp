using Domain.Common;

namespace Domain.ValueObjects;

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}

public sealed class Discount : ValueObject
{
    public DiscountType Type { get; }
    public decimal Value { get; }
    public string? Currency { get; }
    public string? Code { get; }

    private Discount()
    {
        Currency = null;
        Code = null;
    }

    private Discount(DiscountType type, decimal value, string? currency, string? code)
    {
        Type = type;
        Value = value;
        Currency = currency;
        Code = code;
    }

    public static Discount Percentage(decimal percent, string? code = null)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Percentage discount must be between 0 and 100", nameof(percent));

        return new Discount(DiscountType.Percentage, percent, null, code);
    }

    public static Discount FixedAmount(decimal amount, string currency, string? code = null)
    {
        if (amount < 0)
            throw new ArgumentException("Fixed amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

        return new Discount(
            DiscountType.FixedAmount,
            Math.Round(amount, 2, MidpointRounding.ToEven),
            currency.ToUpperInvariant(),
            code);
    }

    public Money ApplyTo(Money original)
    {
        if (original is null)
            throw new ArgumentNullException(nameof(original));

        if (Type == DiscountType.Percentage)
        {
            var percent = new Percentage(Value);
            var discountAmount = percent.ApplyTo(original);
            return original.Subtract(discountAmount);
        }

        if (!string.Equals(Currency, original.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Cannot apply {Currency} discount to {original.Currency} amount.");

        if (Value >= original.Amount)
            return new Money(0, original.Currency);

        return original.Subtract(new Money(Value, Currency!));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
        yield return Currency ?? string.Empty;
        yield return Code ?? string.Empty;
    }

    public override string ToString() =>
        Type == DiscountType.Percentage
            ? $"{Value}%" + (Code is null ? string.Empty : $" ({Code})")
            : $"{Currency} {Value:F2}" + (Code is null ? string.Empty : $" ({Code})");
}
