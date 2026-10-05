using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public interface IAuditableRequest
{
    string AuditAction { get; }
    string AuditEntityType { get; }
    string? AuditEntityId { get; }
}

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuditableRequest auditable)
            return await next();

        var stopwatch = Stopwatch.StartNew();
        var payload = TrySerialize(request);

        try
        {
            var response = await next();
            stopwatch.Stop();

            await _auditService.LogAsync(new AuditEntry(
                Action: auditable.AuditAction,
                EntityType: auditable.AuditEntityType,
                EntityId: auditable.AuditEntityId,
                Success: true,
                UserId: _currentUser.UserId,
                Username: _currentUser.Username,
                CorrelationId: _currentUser.CorrelationId,
                IpAddress: _currentUser.IpAddress,
                Payload: payload,
                DurationMs: stopwatch.ElapsedMilliseconds), cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await _auditService.LogAsync(new AuditEntry(
                Action: auditable.AuditAction,
                EntityType: auditable.AuditEntityType,
                EntityId: auditable.AuditEntityId,
                Success: false,
                UserId: _currentUser.UserId,
                Username: _currentUser.Username,
                CorrelationId: _currentUser.CorrelationId,
                IpAddress: _currentUser.IpAddress,
                Payload: payload,
                ErrorMessage: ex.Message,
                DurationMs: stopwatch.ElapsedMilliseconds), cancellationToken);

            _logger.LogWarning(ex, "Auditable request {Request} failed", typeof(TRequest).Name);
            throw;
        }
    }

    private static string? TrySerialize(TRequest request)
    {
        try
        {
            return JsonSerializer.Serialize(request, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
