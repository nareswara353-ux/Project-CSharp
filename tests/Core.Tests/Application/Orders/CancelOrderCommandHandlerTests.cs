using Application.Orders;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(_orderRepo.Object);
    }

    private Order CreateDraftOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.AddLine(Guid.NewGuid(), "Widget", new Money(10m, "USD"), new Quantity(1));
        return order;
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenOrderIsDraft()
    {
        var order = CreateDraftOrder();
        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepo.Verify(r => r.Update(order), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenOrderIsConfirmed()
    {
        var order = CreateDraftOrder();
        order.Confirm();
        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var command = new CancelOrderCommand { OrderId = Guid.NewGuid() };

        _orderRepo.Setup(r => r.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderAlreadyShipped()
    {
        var order = CreateDraftOrder();
        order.Confirm();
        order.Ship();
        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        result.Error.Should().Contain("already been shipped");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderAlreadyCancelled()
    {
        var order = CreateDraftOrder();
        order.Cancel();
        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        result.Error.Should().Contain("already cancelled");
    }
}
