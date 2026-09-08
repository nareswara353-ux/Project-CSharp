using Application.Common;
using Application.Customers;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Customers;

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _repositoryMock;
    private readonly UpdateCustomerCommandHandler _handler;
    private readonly Email _email;
    private readonly Address _address;

    public UpdateCustomerCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Customer>>();
        _handler = new UpdateCustomerCommandHandler(_repositoryMock.Object);
        _email = Email.Create("john.doe@example.com");
        _address = new Address("123 Main St", "New York", "NY", "10001", "USA");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCustomerUpdated()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        var command = new UpdateCustomerCommand
        {
            Id = customer.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Street = "456 Oak Ave",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.Update(It.IsAny<Customer>()));
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.Update(customer), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.Email.Value.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCustomerNotFound()
    {
        // Arrange
        var command = new UpdateCustomerCommand
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Street = "456 Oak Ave",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Customer with ID {command.Id} not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailInvalid()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        var command = new UpdateCustomerCommand
        {
            Id = customer.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "invalid-email",
            Street = "456 Oak Ave",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Validation error");
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAddressInvalid()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        var command = new UpdateCustomerCommand
        {
            Id = customer.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Street = "", // Invalid
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Validation error");
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        var command = new UpdateCustomerCommand
        {
            Id = customer.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Street = "456 Oak Ave",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update customer");
        result.ErrorCode.Should().Be("UPDATE_FAILED");
        _repositoryMock.Verify(r => r.Update(customer), Times.Once);
    }
}
