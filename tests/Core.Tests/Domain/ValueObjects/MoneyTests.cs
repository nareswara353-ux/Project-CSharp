using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreateMoney_WhenValidParameters()
    {
        // Act
        var money = new Money(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_ShouldThrowException_WhenAmountNegative(decimal amount)
    {
        // Act
        Action act = () => new Money(amount, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("US")]
    [InlineData("USDollar")]
    public void Constructor_ShouldThrowException_WhenCurrencyInvalid(string currency)
    {
        // Act
        Action act = () => new Money(100m, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency*");
    }

    [Fact]
    public void Add_ShouldSumAmounts_WhenSameCurrency()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50.50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_ShouldThrowException_WhenDifferentCurrencies()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add money with different currencies*");
    }

    [Fact]
    public void Subtract_ShouldSubtractAmounts_WhenSameCurrency()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30.25m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(69.75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ShouldMultiplyAmount_WhenValidMultiplier()
    {
        // Arrange
        var money = new Money(10m, "USD");

        // Act
        var result = money.Multiply(2.5m);

        // Assert
        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenSameAmountAndCurrency()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Assert
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenDifferentAmountOrCurrency()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(200m, "USD");
        var money3 = new Money(100m, "EUR");

        // Assert
        money1.Equals(money2).Should().BeFalse();
        money1.Equals(money3).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(123.45m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("USD 123.45");
    }
}
