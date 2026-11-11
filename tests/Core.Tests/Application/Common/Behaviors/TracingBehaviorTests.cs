using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;

namespace Core.Tests.Application.Common.Behaviors;

public class TracingBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenNoException()
    {
        var behavior = new TracingBehavior<TestRequest, string>();

        var response = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenNextThrows()
    {
        var behavior = new TracingBehavior<TestRequest, string>();

        Func<Task> act = async () => await behavior.Handle(
            new TestRequest(),
            _ => throw new InvalidOperationException("test"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test");
    }
}
