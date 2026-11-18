using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class ProductStockLowEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldLogWarning_WhenEventReceived()
    {
        var loggerMock = new Mock<ILogger<ProductStockLowEventHandler>>();
        var handler = new ProductStockLowEventHandler(loggerMock.Object);

        var productId = Guid.NewGuid();
        var domainEvent = new ProductStockLowEvent(productId, "Super Widget", 5, 10);
        var notification = new DomainEventNotification<ProductStockLowEvent>(domainEvent);

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Low stock alert for product {productId}") &&
                    v.ToString()!.Contains("Super Widget") &&
                    v.ToString()!.Contains("5/10")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
