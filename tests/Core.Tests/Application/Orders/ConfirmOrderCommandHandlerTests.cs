using Application.Orders;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Orders;

public class ConfirmOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        _handler = new ConfirmOrderCommandHandler(_orderRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldConfirmOrder_WhenOrderHasLines()
    {
        var order = new Order(Guid.NewGuid());
        order.AddLine(Guid.NewGuid(), "Widget", new Money(10m, "USD"), new Quantity(1));
        var command = new ConfirmOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetWithLinesAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        _orderRepo.Verify(r => r.Update(order), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        var command = new ConfirmOrderCommand { OrderId = Guid.NewGuid() };

        _orderRepo.Setup(r => r.GetWithLinesAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderHasNoLines()
    {
        var order = new Order(Guid.NewGuid()); // no lines
        var command = new ConfirmOrderCommand { OrderId = order.Id };

        _orderRepo.Setup(r => r.GetWithLinesAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        result.Error.Should().Contain("no line items");
        _orderRepo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }
}
