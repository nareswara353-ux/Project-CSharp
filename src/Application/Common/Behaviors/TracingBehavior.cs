using System.Diagnostics;
using Application.Common.Diagnostics;
using MediatR;

namespace Application.Common.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var spanName = $"MediatR.{requestName}";

        using var activity = ActivitySources.Application.StartActivity(spanName, ActivityKind.Internal);
        activity?.SetTag("mediatr.request.name", requestName);
        activity?.SetTag("mediatr.request.type", typeof(TRequest).FullName);

        try
        {
            var response = await next();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
