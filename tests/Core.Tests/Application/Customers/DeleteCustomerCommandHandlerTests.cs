using Application.Customers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Customers;

public class DeleteCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _repositoryMock = new();
    private readonly DeleteCustomerCommandHandler _handler;
    private readonly Customer _customer;

    public DeleteCustomerCommandHandlerTests()
    {
        _handler = new DeleteCustomerCommandHandler(_repositoryMock.Object);
        _customer = new Customer(
            "John",
            "Doe",
            Email.Create("john@example.com"),
            new Address("123 Main St", "New York", "NY", "10001", "USA"));
    }

    [Fact]
    public async Task Handle_ShouldDeactivateCustomer_WhenCustomerExists()
    {
        var command = new DeleteCustomerCommand { Id = _customer.Id };

        _repositoryMock.Setup(r => r.GetByIdAsync(_customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_customer);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _customer.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(_customer), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCustomerNotFound()
    {
        var command = new DeleteCustomerCommand { Id = Guid.NewGuid() };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
