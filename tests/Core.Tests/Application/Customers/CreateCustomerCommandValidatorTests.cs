using Application.Customers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Core.Tests.Application.Customers;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveNoErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = new string('A', 51),
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 50 characters");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmailEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmailFormatInvalid()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldHaveError_WhenStreetEmpty(string street)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = street,
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street)
            .WithErrorMessage("Street is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldHaveError_WhenCityEmpty(string city)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = city,
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorMessage("City is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldHaveError_WhenStateEmpty(string state)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = state,
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State)
            .WithErrorMessage("State is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldHaveError_WhenPostalCodeEmpty(string postalCode)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = postalCode,
            Country = "USA"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostalCode)
            .WithErrorMessage("Postal code is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldHaveError_WhenCountryEmpty(string country)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = country
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country)
            .WithErrorMessage("Country is required");
    }
}
