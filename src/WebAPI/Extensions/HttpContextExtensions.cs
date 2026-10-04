using WebAPI.Common;
using WebAPI.Middleware;

namespace WebAPI.Extensions;

public static class HttpContextExtensions
{
    public static string? GetCorrelationId(this HttpContext context)
        => context.Items[CorrelationIdMiddleware.ItemKey]?.ToString();

    public static Guid? GetUserId(this HttpContext context)
        => context.User.GetUserId();

    public static string? GetUsername(this HttpContext context)
        => context.User.GetUsername();

    public static string? GetUserEmail(this HttpContext context)
        => context.User.GetEmail();

    public static bool IsAuthenticated(this HttpContext context)
        => context.User.Identity?.IsAuthenticated ?? false;

    public static RequestMetadata GetRequestMetadata(this HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();

        var userAgent = context.Request.Headers.UserAgent.ToString();

        return new RequestMetadata(
            context.GetCorrelationId(),
            context.GetUserId(),
            context.GetUsername(),
            ipAddress,
            string.IsNullOrWhiteSpace(userAgent) ? null : userAgent);
    }
}
