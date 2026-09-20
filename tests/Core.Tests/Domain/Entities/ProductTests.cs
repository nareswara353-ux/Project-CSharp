using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class ProductTests
{
    private readonly ProductSku _sku = ProductSku.Create("PRD-001");
    private readonly Money _price = new(100m, "USD");

    [Fact]
    public void Constructor_ShouldCreateProduct_WhenValidParameters()
    {
        var product = new Product("Widget", "A fine widget", _sku, _price, 10);

        product.Name.Should().Be("Widget");
        product.Description.Should().Be("A fine widget");
        product.Sku.Should().Be(_sku);
        product.Price.Should().Be(_price);
        product.StockQuantity.Should().Be(10);
        product.IsActive.Should().BeTrue();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenNameInvalid(string? name)
    {
        Action act = () => new Product(name!, "desc", _sku, _price);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameTooLong()
    {
        var name = new string('A', 201);

        Action act = () => new Product(name, "desc", _sku, _price);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*exceed 200*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSkuNull()
    {
        Action act = () => new Product("Widget", "desc", null!, _price);

        act.Should().Throw<ArgumentNullException>().WithMessage("*sku*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPriceNull()
    {
        Action act = () => new Product("Widget", "desc", _sku, null!);

        act.Should().Throw<ArgumentNullException>().WithMessage("*price*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenStockNegative()
    {
        Action act = () => new Product("Widget", "desc", _sku, _price, -1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Stock quantity cannot be negative*");
    }

    [Fact]
    public void UpdateName_ShouldChangeName()
    {
        var product = new Product("Widget", "desc", _sku, _price);

        product.UpdateName("New Widget");

        product.Name.Should().Be("New Widget");
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_ShouldChangePrice()
    {
        var product = new Product("Widget", "desc", _sku, _price);
        var newPrice = new Money(150m, "USD");

        product.UpdatePrice(newPrice);

        product.Price.Amount.Should().Be(150m);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddStock_ShouldIncreaseQuantity()
    {
        var product = new Product("Widget", "desc", _sku, _price, 10);

        product.AddStock(5);

        product.StockQuantity.Should().Be(15);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddStock_ShouldThrowException_WhenQuantityNotPositive(int qty)
    {
        var product = new Product("Widget", "desc", _sku, _price, 10);

        Action act = () => product.AddStock(qty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void RemoveStock_ShouldDecreaseQuantity()
    {
        var product = new Product("Widget", "desc", _sku, _price, 10);

        product.RemoveStock(4);

        product.StockQuantity.Should().Be(6);
    }

    [Fact]
    public void RemoveStock_ShouldThrowException_WhenInsufficient()
    {
        var product = new Product("Widget", "desc", _sku, _price, 5);

        Action act = () => product.RemoveStock(10);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var product = new Product("Widget", "desc", _sku, _price);

        product.Deactivate();

        product.IsActive.Should().BeFalse();
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var product = new Product("Widget", "desc", _sku, _price);
        product.Deactivate();

        product.Activate();

        product.IsActive.Should().BeTrue();
    }
}
