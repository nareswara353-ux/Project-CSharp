using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_ShouldCreateAddress_WhenAllFieldsValid()
    {
        // Act
        var address = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.State.Should().Be("NY");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("USA");
    }

    [Theory]
    [InlineData("", "City", "State", "12345", "Country")]
    [InlineData(" ", "City", "State", "12345", "Country")]
    [InlineData(null, "City", "State", "12345", "Country")]
    public void Constructor_ShouldThrowException_WhenStreetEmpty(string street, string city, string state, string postalCode, string country)
    {
        // Act
        Action act = () => new Address(street, city, state, postalCode, country);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Street cannot be empty*");
    }

    [Theory]
    [InlineData("Street", "", "State", "12345", "Country")]
    [InlineData("Street", " ", "State", "12345", "Country")]
    [InlineData("Street", null, "State", "12345", "Country")]
    public void Constructor_ShouldThrowException_WhenCityEmpty(string street, string city, string state, string postalCode, string country)
    {
        // Act
        Action act = () => new Address(street, city, state, postalCode, country);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*City cannot be empty*");
    }

    [Theory]
    [InlineData("Street", "City", "", "12345", "Country")]
    [InlineData("Street", "City", " ", "12345", "Country")]
    [InlineData("Street", "City", null, "12345", "Country")]
    public void Constructor_ShouldThrowException_WhenStateEmpty(string street, string city, string state, string postalCode, string country)
    {
        // Act
        Action act = () => new Address(street, city, state, postalCode, country);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*State cannot be empty*");
    }

    [Theory]
    [InlineData("Street", "City", "State", "", "Country")]
    [InlineData("Street", "City", "State", " ", "Country")]
    [InlineData("Street", "City", "State", null, "Country")]
    public void Constructor_ShouldThrowException_WhenPostalCodeEmpty(string street, string city, string state, string postalCode, string country)
    {
        // Act
        Action act = () => new Address(street, city, state, postalCode, country);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Postal code cannot be empty*");
    }

    [Theory]
    [InlineData("Street", "City", "State", "12345", "")]
    [InlineData("Street", "City", "State", "12345", " ")]
    [InlineData("Street", "City", "State", "12345", null)]
    public void Constructor_ShouldThrowException_WhenCountryEmpty(string street, string city, string state, string postalCode, string country)
    {
        // Act
        Action act = () => new Address(street, city, state, postalCode, country);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Country cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldTrimAndUppercasePostalCode()
    {
        // Act
        var address = new Address("Street", "City", "State", " 12345-6789 ", "Country");

        // Assert
        address.PostalCode.Should().Be("12345-6789");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenAllFieldsSame()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "NY", "10001", "USA");
        var address2 = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Assert
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenAnyFieldDifferent()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "NY", "10001", "USA");
        var address2 = new Address("456 Oak Ave", "New York", "NY", "10001", "USA");

        // Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address("123 Main St", "New York", "NY", "10001", "USA");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("123 Main St, New York, NY 10001, USA");
    }
}
