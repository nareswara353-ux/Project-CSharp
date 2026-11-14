using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class DomainEventLoggingHandlerTests
{
    public record DummyEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    [Fact]
    public async Task Handle_ShouldLogDebugEvent_WhenEventReceived()
    {
        var loggerMock = new Mock<ILogger<DomainEventLoggingHandler<DummyEvent>>>();
        var handler = new DomainEventLoggingHandler<DummyEvent>(loggerMock.Object);

        var domainEvent = new DummyEvent();
        var notification = new DomainEventNotification<DummyEvent>(domainEvent);

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Domain event DummyEvent occurred")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
