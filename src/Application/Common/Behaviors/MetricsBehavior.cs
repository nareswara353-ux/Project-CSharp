using System.Diagnostics;
using Application.Common.Diagnostics;
using MediatR;

namespace Application.Common.Behaviors;

public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command", StringComparison.Ordinal);
        var isQuery = requestName.EndsWith("Query", StringComparison.Ordinal);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (isCommand)
                ApplicationMetrics.CommandsProcessed.Add(1, new KeyValuePair<string, object?>("name", requestName));
            else if (isQuery)
                ApplicationMetrics.QueriesProcessed.Add(1, new KeyValuePair<string, object?>("name", requestName));

            ApplicationMetrics.RequestDuration.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("name", requestName));

            return response;
        }
        catch
        {
            stopwatch.Stop();

            if (isCommand)
                ApplicationMetrics.CommandsFailed.Add(1, new KeyValuePair<string, object?>("name", requestName));

            throw;
        }
    }
}
