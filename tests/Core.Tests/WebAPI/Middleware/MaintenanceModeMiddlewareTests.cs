using System.Text;
using Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebAPI.Middleware;

namespace Core.Tests.WebAPI.Middleware;

public class MaintenanceModeMiddlewareTests
{
    private readonly Mock<IFeatureFlagService> _flags = new();

    private MaintenanceModeMiddleware BuildMiddleware(bool nextCalled, out Func<bool> wasCalled)
    {
        var called = false;
        wasCalled = () => called;

        var middleware = new MaintenanceModeMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            NullLogger<MaintenanceModeMiddleware>.Instance);

        return middleware;
    }

    private static HttpContext BuildContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_ShouldBlock_WhenMaintenanceEnabled()
    {
        _flags.Setup(f => f.IsEnabledAsync(FeatureFlags.MaintenanceMode, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = BuildMiddleware(false, out var wasCalled);
        var context = BuildContext("/api/v1/customers");

        await middleware.InvokeAsync(context, _flags.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        wasCalled().Should().BeFalse();

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("MAINTENANCE_MODE");
        context.Response.Headers["Retry-After"].ToString().Should().Be("300");
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassThrough_WhenMaintenanceDisabled()
    {
        _flags.Setup(f => f.IsEnabledAsync(FeatureFlags.MaintenanceMode, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var middleware = BuildMiddleware(false, out var wasCalled);
        var context = BuildContext("/api/v1/customers");

        await middleware.InvokeAsync(context, _flags.Object);

        wasCalled().Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/api/v1/auth/login")]
    [InlineData("/api/v1/auth/register")]
    [InlineData("/api/v1/featureflags")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_ShouldBypass_WhenBypassedPath(string path)
    {
        _flags.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = BuildMiddleware(false, out var wasCalled);
        var context = BuildContext(path);

        await middleware.InvokeAsync(context, _flags.Object);

        wasCalled().Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAllowThrough_WhenFlagServiceThrows()
    {
        _flags.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("flag service down"));

        var middleware = BuildMiddleware(false, out var wasCalled);
        var context = BuildContext("/api/v1/customers");

        await middleware.InvokeAsync(context, _flags.Object);

        wasCalled().Should().BeTrue();
    }
}
