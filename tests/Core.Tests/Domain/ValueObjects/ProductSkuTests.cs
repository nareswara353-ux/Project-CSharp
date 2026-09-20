using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.ValueObjects;

public class ProductSkuTests
{
    [Theory]
    [InlineData("PRD-001")]
    [InlineData("ABC123")]
    [InlineData("SKU-A-1-B-2")]
    [InlineData("ABC")]
    public void Create_ShouldReturnSku_WhenFormatValid(string input)
    {
        var sku = ProductSku.Create(input);

        sku.Value.Should().Be(input.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenEmpty(string? input)
    {
        Action act = () => ProductSku.Create(input!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*SKU cannot be empty*");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A")]
    public void Create_ShouldThrowException_WhenTooShort(string input)
    {
        Action act = () => ProductSku.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 3 and 50*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTooLong()
    {
        var input = new string('A', 51);

        Action act = () => ProductSku.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 3 and 50*");
    }

    [Theory]
    [InlineData("PRD 001")]
    [InlineData("PRD_001")]
    [InlineData("PRD.001")]
    [InlineData("prd@001")]
    [InlineData("-PRD001")]
    [InlineData("PRD001-")]
    public void Create_ShouldThrowException_WhenInvalidCharacters(string input)
    {
        Action act = () => ProductSku.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*uppercase letters, digits, and dashes*");
    }

    [Fact]
    public void Create_ShouldNormalizeToUppercase()
    {
        var sku = ProductSku.Create("prd-001");

        sku.Value.Should().Be("PRD-001");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var sku = ProductSku.Create("  PRD-001  ");

        sku.Value.Should().Be("PRD-001");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenSameValue()
    {
        var sku1 = ProductSku.Create("PRD-001");
        var sku2 = ProductSku.Create("prd-001");

        sku1.Equals(sku2).Should().BeTrue();
        (sku1 == sku2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenDifferentValue()
    {
        var sku1 = ProductSku.Create("PRD-001");
        var sku2 = ProductSku.Create("PRD-002");

        sku1.Equals(sku2).Should().BeFalse();
        (sku1 != sku2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnString()
    {
        var sku = ProductSku.Create("PRD-001");

        string value = sku;

        value.Should().Be("PRD-001");
    }
}
