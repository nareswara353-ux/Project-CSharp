using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage() { }

    public Percentage(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Percentage cannot be negative", nameof(value));
        if (value > 100)
            throw new ArgumentException("Percentage cannot exceed 100", nameof(value));

        Value = Math.Round(value, 4, MidpointRounding.ToEven);
    }

    public static Percentage Zero => new(0);
    public static Percentage Full => new(100);

    public static Percentage FromFraction(decimal fraction)
    {
        if (fraction < 0 || fraction > 1)
            throw new ArgumentException("Fraction must be between 0 and 1", nameof(fraction));

        return new Percentage(fraction * 100);
    }

    public decimal AsFraction() => Value / 100m;

    public Money ApplyTo(Money amount)
    {
        if (amount is null)
            throw new ArgumentNullException(nameof(amount));

        var calculated = amount.Amount * AsFraction();
        return new Money(Math.Round(calculated, 2, MidpointRounding.ToEven), amount.Currency);
    }

    public Money Discount(Money originalAmount)
    {
        if (originalAmount is null)
            throw new ArgumentNullException(nameof(originalAmount));

        var discount = ApplyTo(originalAmount);
        return originalAmount.Subtract(discount);
    }

    public Money AddTo(Money originalAmount)
    {
        if (originalAmount is null)
            throw new ArgumentNullException(nameof(originalAmount));

        var addition = ApplyTo(originalAmount);
        return originalAmount.Add(addition);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}%";
}
