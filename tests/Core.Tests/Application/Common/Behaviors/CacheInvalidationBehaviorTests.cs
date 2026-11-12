using Application.Common;
using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class CacheInvalidationBehaviorTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<CacheInvalidationBehavior<InvalidatingRequest, string>>> _invalidatingLoggerMock = new();
    private readonly Mock<ILogger<CacheInvalidationBehavior<NormalRequest, string>>> _normalLoggerMock = new();

    public record InvalidatingRequest : IRequest<string>, ICacheInvalidation
    {
        public IEnumerable<string> CacheKeysToInvalidate => new[] { "key1", "key2" };
    }

    public record NormalRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldNotInvalidateCache_WhenRequestIsNotInvalidating()
    {
        var behavior = new CacheInvalidationBehavior<NormalRequest, string>(_cacheMock.Object, _normalLoggerMock.Object);

        var response = await behavior.Handle(
            new NormalRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCacheKeys_WhenRequestIsInvalidating()
    {
        var behavior = new CacheInvalidationBehavior<InvalidatingRequest, string>(_cacheMock.Object, _invalidatingLoggerMock.Object);

        var response = await behavior.Handle(
            new InvalidatingRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        
        _cacheMock.Verify(c => c.RemoveAsync("key1", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("key2", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenNextThrows()
    {
        var behavior = new CacheInvalidationBehavior<InvalidatingRequest, string>(_cacheMock.Object, _invalidatingLoggerMock.Object);

        Func<Task> act = async () => await behavior.Handle(
            new InvalidatingRequest(),
            _ => throw new InvalidOperationException("test error"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test error");

        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
