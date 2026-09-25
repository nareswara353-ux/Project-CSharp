using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Filters;

public sealed class FeatureFlagAuthorizationFilter : IAsyncActionFilter
{
    private readonly IFeatureFlagService _featureFlags;
    private readonly string _flagName;

    public FeatureFlagAuthorizationFilter(
        IFeatureFlagService featureFlags,
        string flagName)
    {
        _featureFlags = featureFlags;
        _flagName = flagName;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var enabled = await _featureFlags.IsEnabledAsync(
            _flagName,
            defaultValue: false,
            context.HttpContext.RequestAborted);

        if (!enabled)
        {
            context.Result = new NotFoundObjectResult(new
            {
                error = "Feature not available",
                code = "FEATURE_DISABLED",
                flag = _flagName
            });
            return;
        }

        await next();
    }
}
