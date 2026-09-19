using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity() { } // For EF Core

    public Quantity(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(value));

        Value = value;
    }

    public static Quantity Zero => new(0);

    public static Quantity Of(int value) => new(value);

    public Quantity Add(Quantity other)
    {
        return new Quantity(Value + other.Value);
    }

    public Quantity Subtract(Quantity other)
    {
        if (Value < other.Value)
            throw new InvalidOperationException(
                $"Cannot subtract {other.Value} from {Value}: result would be negative");

        return new Quantity(Value - other.Value);
    }

    public Quantity Multiply(int multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Quantity(Value * multiplier);
    }

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(Quantity quantity) => quantity.Value;
}
