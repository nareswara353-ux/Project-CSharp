using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class UnhandledExceptionBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenNoException()
    {
        var loggerMock = new Mock<ILogger<UnhandledExceptionBehavior<TestRequest, string>>>();
        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(loggerMock.Object);

        var response = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLogErrorAndRethrow_WhenExceptionThrown()
    {
        var loggerMock = new Mock<ILogger<UnhandledExceptionBehavior<TestRequest, string>>>();
        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(loggerMock.Object);

        Func<Task> act = async () => await behavior.Handle(
            new TestRequest(),
            _ => throw new InvalidOperationException("test exception"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test exception");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled Exception for Request TestRequest")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
