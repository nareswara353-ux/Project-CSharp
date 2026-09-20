using System.Diagnostics;

namespace WebAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
        var user = context.User?.Identity?.Name ?? "anonymous";

        try
        {
            _logger.LogInformation(
                "HTTP {Method} {Path} started - CorrelationId: {CorrelationId}, User: {User}",
                method,
                path,
                correlationId,
                user);

            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var level = statusCode >= 500
                ? LogLevel.Error
                : statusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms - CorrelationId: {CorrelationId}",
                method,
                path,
                statusCode,
                elapsedMs,
                correlationId);
        }
    }
}
