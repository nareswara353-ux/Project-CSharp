using Application.Pricing;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Application.Pricing;

public class PriceCalculatorTests
{
    private readonly PriceCalculator _calculator = new();

    [Fact]
    public void CalculateTotal_ShouldReturnSubtotal_WhenNoDiscount()
    {
        var subtotal = new Money(100m, "USD");

        var result = _calculator.CalculateTotal(subtotal);

        result.Amount.Should().Be(100m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void CalculateTotal_ShouldApplyPercentageDiscount()
    {
        var subtotal = new Money(100m, "USD");
        var discount = Discount.Percentage(10m);

        var result = _calculator.CalculateTotal(subtotal, discount);

        result.Amount.Should().Be(90m);
    }

    [Fact]
    public void CalculateTotal_ShouldApplyFixedAmountDiscount()
    {
        var subtotal = new Money(100m, "USD");
        var discount = Discount.FixedAmount(15m, "USD");

        var result = _calculator.CalculateTotal(subtotal, discount);

        result.Amount.Should().Be(85m);
    }

    [Fact]
    public void CalculateTotalWithTax_ShouldApplyDiscountThenTax()
    {
        var subtotal = new Money(100m, "USD");
        var discount = Discount.Percentage(10m);
        var tax = new Percentage(8.5m);

        var result = _calculator.CalculateTotalWithTax(subtotal, discount, tax);

        result.Amount.Should().Be(97.65m);
    }

    [Fact]
    public void CalculateTotal_ShouldClampToZero_WhenFixedDiscountExceedsSubtotal()
    {
        var subtotal = new Money(20m, "USD");
        var discount = Discount.FixedAmount(50m, "USD");

        var result = _calculator.CalculateTotal(subtotal, discount);

        result.Amount.Should().Be(0m);
    }
}
