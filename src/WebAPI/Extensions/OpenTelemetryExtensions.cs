using System.Diagnostics;
using Application.Common.Diagnostics;
using Infrastructure.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebAPI.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>() ?? new ObservabilitySettings();

        if (!settings.Enabled)
            return services;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: settings.ServiceName,
                    serviceVersion: settings.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource(ActivitySources.ApplicationName)
                    .SetSampler(new AlwaysOnSampler());

                if (!string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                        options.Endpoint = new Uri(settings.OtlpEndpoint));
                }

                if (settings.EnableConsoleExporter)
                    tracing.AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ApplicationMetrics.MeterName);

                if (!string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                        options.Endpoint = new Uri(settings.OtlpEndpoint));
                }

                if (settings.EnableConsoleExporter)
                    metrics.AddConsoleExporter();
            });

        return services;
    }
}
