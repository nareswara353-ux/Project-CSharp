using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class OrderCreatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldLogInformation_WhenEventReceived()
    {
        var loggerMock = new Mock<ILogger<OrderCreatedEventHandler>>();
        var handler = new OrderCreatedEventHandler(loggerMock.Object);

        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var domainEvent = new OrderCreatedEvent(orderId, customerId, 150.50m, "USD");
        var notification = new DomainEventNotification<OrderCreatedEvent>(domainEvent);

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Order {orderId} created for customer {customerId}") &&
                    v.ToString()!.Contains("150.50") &&
                    v.ToString()!.Contains("USD")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
