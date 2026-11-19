using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class CustomerUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldLogInformation_WhenEventReceived()
    {
        var loggerMock = new Mock<ILogger<CustomerUpdatedEventHandler>>();
        var handler = new CustomerUpdatedEventHandler(loggerMock.Object);

        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerUpdatedEvent(customerId, "Jane Smith", "jane@example.com");
        var notification = new DomainEventNotification<CustomerUpdatedEvent>(domainEvent);

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Customer {customerId} updated") &&
                    v.ToString()!.Contains("Jane Smith") &&
                    v.ToString()!.Contains("jane@example.com")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
