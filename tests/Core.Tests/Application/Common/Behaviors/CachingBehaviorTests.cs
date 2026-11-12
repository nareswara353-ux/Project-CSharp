using Application.Common;
using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class CachingBehaviorTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<CachingBehavior<CacheableRequest, string>>> _cacheableLoggerMock = new();
    private readonly Mock<ILogger<CachingBehavior<NonCacheableRequest, string>>> _nonCacheableLoggerMock = new();

    public record CacheableRequest(string Key) : IRequest<string>, ICacheableQuery
    {
        public string CacheKey => Key;
        public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(5);
    }

    public record NonCacheableRequest : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenCacheHit()
    {
        var request = new CacheableRequest("test-key");
        var behavior = new CachingBehavior<CacheableRequest, string>(_cacheMock.Object, _cacheableLoggerMock.Object);

        _cacheMock.Setup(c => c.GetAsync<string>("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cached-value");

        var nextCalled = false;
        var response = await behavior.Handle(
            request,
            _ => { nextCalled = true; return Task.FromResult("fresh-value"); },
            CancellationToken.None);

        response.Should().Be("cached-value");
        nextCalled.Should().BeFalse();
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallNextAndCacheResponse_WhenCacheMiss()
    {
        var request = new CacheableRequest("test-key");
        var behavior = new CachingBehavior<CacheableRequest, string>(_cacheMock.Object, _cacheableLoggerMock.Object);

        _cacheMock.Setup(c => c.GetAsync<string>("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var nextCalled = false;
        var response = await behavior.Handle(
            request,
            _ => { nextCalled = true; return Task.FromResult("fresh-value"); },
            CancellationToken.None);

        response.Should().Be("fresh-value");
        nextCalled.Should().BeTrue();
        _cacheMock.Verify(c => c.SetAsync("test-key", "fresh-value", TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotUseCache_WhenRequestIsNotCacheable()
    {
        var request = new NonCacheableRequest();
        var behavior = new CachingBehavior<NonCacheableRequest, string>(_cacheMock.Object, _nonCacheableLoggerMock.Object);

        var nextCalled = false;
        var response = await behavior.Handle(
            request,
            _ => { nextCalled = true; return Task.FromResult("fresh-value"); },
            CancellationToken.None);

        response.Should().Be("fresh-value");
        nextCalled.Should().BeTrue();
        _cacheMock.Verify(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
