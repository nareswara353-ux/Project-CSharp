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
}
