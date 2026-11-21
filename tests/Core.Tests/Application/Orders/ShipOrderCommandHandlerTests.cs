using Application.Orders;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Orders;

public class ShipOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly ShipOrderCommandHandler _handler;

    public ShipOrderCommandHandlerTests()
    {
        _handler = new ShipOrderCommandHandler(_orderRepo.Object);
    }

    private Order CreateConfirmedOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.AddLine(Guid.NewGuid(), "Widget", new Money(10m, "USD"), new Quantity(1));
        order.Confirm();
        return order;
    }

    [Fact]
    public async Task Handle_ShouldShipOrder_WhenOrderIsConfirmed()
    {
        var order = CreateConfirmedOrder();
        var command = new ShipOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
        _orderRepo.Verify(r => r.Update(order), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var command = new ShipOrderCommand { OrderId = Guid.NewGuid() };

        _orderRepo.Setup(r => r.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotConfirmed()
    {
        var order = new Order(Guid.NewGuid()); // Draft status
        var command = new ShipOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        result.Error.Should().Contain("Only Confirmed orders can be shipped");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderAlreadyShipped()
    {
        var order = CreateConfirmedOrder();
        order.Ship(); // Already shipped
        var command = new ShipOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
    }
}
