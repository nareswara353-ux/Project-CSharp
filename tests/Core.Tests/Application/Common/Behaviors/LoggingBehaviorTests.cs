using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class LoggingBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenSuccessful()
    {
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);

        var response = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionThrown()
    {
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);

        Func<Task> act = async () => await behavior.Handle(
            new TestRequest(),
            _ => throw new InvalidOperationException("test error"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request TestRequest failed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
