using Application.Orders;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IRepository<Customer>> _customerRepo = new();
    private readonly CreateOrderCommandHandler _handler;
    private readonly Customer _customer;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_orderRepo.Object, _customerRepo.Object);
        _customer = new Customer(
            "John",
            "Doe",
            Email.Create("john@example.com"),
            new Address("123 Main St", "New York", "NY", "10001", "USA"));
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValidOrderCreated()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(_customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_customer);
        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _orderRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var command = new CreateOrderCommand
        {
            CustomerId = _customer.Id,
            Notes = "Urgent order",
            Lines = new List<OrderLineRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    UnitPrice = 10m,
                    Currency = "USD",
                    Quantity = 2
                }
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCustomerNotFound()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Lines = new List<OrderLineRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    UnitPrice = 10m,
                    Currency = "USD",
                    Quantity = 1
                }
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CUSTOMER_NOT_FOUND");
        _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuantityZero()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(_customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_customer);

        var command = new CreateOrderCommand
        {
            CustomerId = _customer.Id,
            Lines = new List<OrderLineRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    UnitPrice = 10m,
                    Currency = "USD",
                    Quantity = 0
                }
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}
