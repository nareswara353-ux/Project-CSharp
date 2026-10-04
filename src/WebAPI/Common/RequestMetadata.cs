namespace WebAPI.Common;

public record RequestMetadata(
    string? CorrelationId,
    Guid? UserId,
    string? Username,
    string? IpAddress,
    string? UserAgent)
{
    public static RequestMetadata Empty => new(null, null, null, null, null);
}
