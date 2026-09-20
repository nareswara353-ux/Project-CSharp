using System.Net;
using System.Text.Json;
using FluentValidation;
using WebAPI.Common;

namespace WebAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(
                validationException,
                "Validation failed for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                ApiResponse.Failure(
                    validationException.Errors.Select(e => e.ErrorMessage).ToList(),
                    "VALIDATION_ERROR",
                    StatusCodes.Status400BadRequest));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            var statusCode = MapStatusCode(exception);

            var message = _environment.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred. Please try again later.";

            await WriteResponseAsync(
                context,
                statusCode,
                ApiResponse.Failure(message, exception.GetType().Name, (int)statusCode));
        }
    }

    private static HttpStatusCode MapStatusCode(Exception exception) => exception switch
    {
        ArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        KeyNotFoundException => HttpStatusCode.NotFound,
        InvalidOperationException => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };

    private static async Task WriteResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        ApiResponse response)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var correlationId = context.Items["CorrelationId"]?.ToString();
        if (!string.IsNullOrWhiteSpace(correlationId))
            context.Response.Headers["X-Correlation-Id"] = correlationId;

        var json = JsonSerializer.Serialize(response, SerializerOptions);
        await context.Response.WriteAsync(json);
    }
}
