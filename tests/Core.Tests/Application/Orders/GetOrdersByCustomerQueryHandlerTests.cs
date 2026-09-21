using Application.Orders;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Orders;

public class GetOrdersByCustomerQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly GetOrdersByCustomerQueryHandler _handler;
    private readonly Guid _customerId = Guid.NewGuid();

    public GetOrdersByCustomerQueryHandlerTests()
    {
        _handler = new GetOrdersByCustomerQueryHandler(_orderRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrders()
    {
        _orderRepo.Setup(r => r.GetByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var result = await _handler.Handle(
            new GetOrdersByCustomerQuery { CustomerId = _customerId },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrders_WhenOrdersExist()
    {
        var order1 = new Order(_customerId);
        order1.AddLine(Guid.NewGuid(), "Widget", new Money(10m, "USD"), new Quantity(2));

        var order2 = new Order(_customerId);
        order2.AddLine(Guid.NewGuid(), "Gadget", new Money(20m, "USD"), new Quantity(1));

        _orderRepo.Setup(r => r.GetByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order1, order2 });

        var result = await _handler.Handle(
            new GetOrdersByCustomerQuery { CustomerId = _customerId },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        _orderRepo.Setup(r => r.GetByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(
            new GetOrdersByCustomerQuery { CustomerId = _customerId },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GET_ORDERS_FAILED");
    }
}
