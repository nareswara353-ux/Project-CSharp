namespace Application.Common;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? IpAddress { get; }
    string? CorrelationId { get; }
    bool IsAuthenticated { get; }
}
