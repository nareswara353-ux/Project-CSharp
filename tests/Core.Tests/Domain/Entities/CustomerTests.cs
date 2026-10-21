using Domain.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class CustomerTests
{
    private readonly Email _email;
    private readonly Address _address;

    public CustomerTests()
    {
        _email = Email.Create("john.doe@example.com");
        _address = new Address("123 Main St", "New York", "NY", "10001", "USA");
    }

    [Fact]
    public void Constructor_ShouldCreateCustomer_WhenValidParameters()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be(_email);
        customer.BillingAddress.Should().Be(_address);
        customer.ShippingAddress.Should().BeNull();
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.UpdatedAt.Should().BeNull();
        customer.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData(" ", "Doe")]
    [InlineData(null, "Doe")]
    [InlineData("John", "")]
    [InlineData("John", " ")]
    [InlineData("John", null)]
    public void Constructor_ShouldThrowException_WhenNameInvalid(string? firstName, string? lastName)
    {
        Action act = () => new Customer(firstName!, lastName!, _email, _address);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailNull()
    {
        Action act = () => new Customer("John", "Doe", null!, _address);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenBillingAddressNull()
    {
        Action act = () => new Customer("John", "Doe", _email, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*BillingAddress*");
    }

    [Fact]
    public void Constructor_ShouldSetShippingAddress_WhenProvided()
    {
        var shippingAddress = new Address("456 Oak Ave", "Los Angeles", "CA", "90210", "USA");

        var customer = new Customer("John", "Doe", _email, _address, shippingAddress);

        customer.ShippingAddress.Should().Be(shippingAddress);
    }

    [Fact]
    public void UpdateName_ShouldChangeName_WhenValid()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        customer.UpdateName("Jane", "Smith");

        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.FullName.Should().Be("Jane Smith");
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("Jane", "")]
    public void UpdateName_ShouldThrowException_WhenNameInvalid(string firstName, string lastName)
    {
        var customer = new Customer("John", "Doe", _email, _address);

        Action act = () => customer.UpdateName(firstName, lastName);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void UpdateEmail_ShouldChangeEmail_WhenValid()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        var newEmail = Email.Create("jane.doe@example.com");

        customer.UpdateEmail(newEmail);

        customer.Email.Should().Be(newEmail);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateEmail_ShouldThrowException_WhenEmailNull()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        Action act = () => customer.UpdateEmail(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*newEmail*");
    }

    [Fact]
    public void UpdateBillingAddress_ShouldChangeAddress_WhenValid()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        var newAddress = new Address("789 Pine St", "Chicago", "IL", "60601", "USA");

        customer.UpdateBillingAddress(newAddress);

        customer.BillingAddress.Should().Be(newAddress);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateBillingAddress_ShouldThrowException_WhenAddressNull()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        Action act = () => customer.UpdateBillingAddress(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*newAddress*");
    }

    [Fact]
    public void UpdateShippingAddress_ShouldSetAddress_WhenValid()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        var newShipping = new Address("456 Oak Ave", "Los Angeles", "CA", "90210", "USA");

        customer.UpdateShippingAddress(newShipping);

        customer.ShippingAddress.Should().Be(newShipping);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateShippingAddress_ShouldSetToNull_WhenNullPassed()
    {
        var shippingAddress = new Address("456 Oak Ave", "Los Angeles", "CA", "90210", "USA");
        var customer = new Customer("John", "Doe", _email, _address, shippingAddress);

        customer.UpdateShippingAddress(null);

        customer.ShippingAddress.Should().BeNull();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetActiveToTrue_WhenCalled()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        customer.Deactivate();

        customer.Activate();

        customer.IsActive.Should().BeTrue();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetActiveToFalse_WhenCalled()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenSameInstance()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        var customerSame = customer;
        var customerDifferent = new Customer("Jane", "Smith", _email, _address);

        customer.Equals(customerSame).Should().BeTrue();
        (customer == customerSame).Should().BeTrue();
        customer.Equals(customerDifferent).Should().BeFalse();
        (customer != customerDifferent).Should().BeTrue();
        customer.Id.Should().NotBe(customerDifferent.Id);
    }

    [Fact]
    public void GetHashCode_ShouldBeSameForSameInstance()
    {
        var customer = new Customer("John", "Doe", _email, _address);
        var customerSame = customer;

        customer.GetHashCode().Should().Be(customerSame.GetHashCode());
    }

    [Fact]
    public void FullName_ShouldReturnFormattedName()
    {
        var customer = new Customer("John", "Doe", _email, _address);

        customer.FullName.Should().Be("John Doe");
    }
}
