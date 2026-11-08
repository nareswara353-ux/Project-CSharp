using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class PerformanceBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldNotLogWarning_WhenExecutionIsFast()
    {
        var loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, string>>>();
        var behavior = new PerformanceBehavior<TestRequest, string>(loggerMock.Object);

        var response = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenExecutionIsSlow()
    {
        var loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, string>>>();
        var behavior = new PerformanceBehavior<TestRequest, string>(loggerMock.Object);

        var response = await behavior.Handle(
            new TestRequest(),
            async _ => 
            { 
                await Task.Delay(600); 
                return "ok"; 
            },
            CancellationToken.None);

        response.Should().Be("ok");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
