using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.ValueObjects;

public class QuantityTests
{
    [Fact]
    public void Constructor_ShouldCreateQuantity_WhenValueNonNegative()
    {
        var quantity = new Quantity(10);

        quantity.Value.Should().Be(10);
    }

    [Fact]
    public void Constructor_ShouldAllowZero()
    {
        var quantity = new Quantity(0);

        quantity.Value.Should().Be(0);
        quantity.IsZero.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ShouldThrowException_WhenNegative(int value)
    {
        Action act = () => new Quantity(value);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity cannot be negative*");
    }

    [Fact]
    public void Add_ShouldSumValues()
    {
        var q1 = new Quantity(5);
        var q2 = new Quantity(3);

        var result = q1.Add(q2);

        result.Value.Should().Be(8);
    }

    [Fact]
    public void Subtract_ShouldReduceValue()
    {
        var q1 = new Quantity(10);
        var q2 = new Quantity(4);

        var result = q1.Subtract(q2);

        result.Value.Should().Be(6);
    }

    [Fact]
    public void Subtract_ShouldThrowException_WhenResultWouldBeNegative()
    {
        var q1 = new Quantity(3);
        var q2 = new Quantity(10);

        Action act = () => q1.Subtract(q2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*result would be negative*");
    }

    [Fact]
    public void Multiply_ShouldMultiplyValue()
    {
        var q = new Quantity(5);

        var result = q.Multiply(3);

        result.Value.Should().Be(15);
    }

    [Fact]
    public void Multiply_ShouldThrowException_WhenMultiplierNegative()
    {
        var q = new Quantity(5);

        Action act = () => q.Multiply(-1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Multiplier cannot be negative*");
    }

    [Fact]
    public void IsPositive_ShouldReturnTrue_WhenValueGreaterThanZero()
    {
        var q = new Quantity(1);

        q.IsPositive.Should().BeTrue();
        q.IsZero.Should().BeFalse();
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenSameValue()
    {
        var q1 = new Quantity(5);
        var q2 = new Quantity(5);

        q1.Equals(q2).Should().BeTrue();
        (q1 == q2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenDifferentValue()
    {
        var q1 = new Quantity(5);
        var q2 = new Quantity(10);

        q1.Equals(q2).Should().BeFalse();
        (q1 != q2).Should().BeTrue();
    }
}
