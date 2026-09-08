using Application.Common;
using Application.Customers;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Customers;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Customer>> _repositoryMock;
    private readonly GetCustomerByIdQueryHandler _handler;
    private readonly Email _email;
    private readonly Address _address;

    public GetCustomerByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Customer>>();
        _handler = new GetCustomerByIdQueryHandler(_repositoryMock.Object);
        _email = Email.Create("john.doe@example.com");
        _address = new Address("123 Main St", "New York", "NY", "10001", "USA");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithCustomerDto_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe", _email, _address);
        // Use reflection to set Id (or we can create a helper, but for test we set via private setter using a method)
        // Since Id is protected set, we can use a test helper or just create via constructor and then assign using reflection.
        // Alternatively, we can create a new Customer with a specific Id using a protected constructor? No.
        // We'll use a mock that returns a customer we create, but the Id is generated. We need to ensure the Id matches the query.
        // We can create a custom method in test to set Id via reflection.
        // Simpler: create customer, then use reflection to set Id.
        typeof(Entity).GetProperty("Id")?.SetValue(customer, customerId);
        // Alternatively, we can just use the generated Id but then the query should use that Id.
        // Actually we can query with the generated Id. So we'll store the customer and use its Id.
        // But we already have a customerId variable; we can set the customer's Id to that using reflection.
        // Let's do that:
        var idProperty = typeof(Entity).GetProperty("Id");
        idProperty?.SetValue(customer, customerId);

        _repositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery { Id = customerId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(customerId);
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("john.doe@example.com");
        result.Value.BillingAddress.Should().Contain("123 Main St");
        result.Value.IsActive.Should().BeTrue();
        _repositoryMock.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WithNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerByIdQuery { Id = customerId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Customer with ID {customerId} not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
        _repositoryMock.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
