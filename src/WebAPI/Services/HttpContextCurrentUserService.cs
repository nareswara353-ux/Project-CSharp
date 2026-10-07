using Application.Common;
using WebAPI.Extensions;

namespace WebAPI.Services;

public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? Context => _httpContextAccessor.HttpContext;

    public Guid? UserId => Context?.GetUserId();

    public string? Username => Context?.GetUsername();

    public string? Email => Context?.GetUserEmail();

    public string? IpAddress
    {
        get
        {
            if (Context is null)
                return null;

            return Context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? Context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? CorrelationId => Context?.GetCorrelationId();

    public bool IsAuthenticated => Context?.IsAuthenticated() ?? false;
}
