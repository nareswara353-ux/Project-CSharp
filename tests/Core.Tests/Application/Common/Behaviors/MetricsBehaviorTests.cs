using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;

namespace Core.Tests.Application.Common.Behaviors;

public class MetricsBehaviorTests
{
    public record TestCommand : IRequest<string>;
    public record TestQuery : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldReturnResponse_ForCommand()
    {
        var behavior = new MetricsBehavior<TestCommand, string>();

        var response = await behavior.Handle(
            new TestCommand(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_ForQuery()
    {
        var behavior = new MetricsBehavior<TestQuery, string>();

        var response = await behavior.Handle(
            new TestQuery(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenNextThrows()
    {
        var behavior = new MetricsBehavior<TestCommand, string>();

        Func<Task> act = async () => await behavior.Handle(
            new TestCommand(),
            _ => throw new InvalidOperationException("test"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test");
    }
}
