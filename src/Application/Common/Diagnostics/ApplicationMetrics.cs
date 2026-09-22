using System.Diagnostics.Metrics;

namespace Application.Common.Diagnostics;

public static class ApplicationMetrics
{
    public const string MeterName = "PortfolioEnterprise.Metrics";

    public static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> CommandsProcessed =
        Meter.CreateCounter<long>(
            "app.commands.processed",
            description: "Total number of commands processed.");

    public static readonly Counter<long> QueriesProcessed =
        Meter.CreateCounter<long>(
            "app.queries.processed",
            description: "Total number of queries processed.");

    public static readonly Counter<long> CommandsFailed =
        Meter.CreateCounter<long>(
            "app.commands.failed",
            description: "Total number of commands that failed.");

    public static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>(
            "app.request.duration",
            unit: "ms",
            description: "Duration of command/query processing in milliseconds.");

    public static readonly Counter<long> OrdersCreated =
        Meter.CreateCounter<long>(
            "app.orders.created",
            description: "Total number of orders created.");

    public static readonly Counter<long> CustomersCreated =
        Meter.CreateCounter<long>(
            "app.customers.created",
            description: "Total number of customers created.");
}
