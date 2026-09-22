namespace Infrastructure.Observability;

public class ObservabilitySettings
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; } = false;
    public string ServiceName { get; set; } = "PortfolioEnterpriseAPI";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string? OtlpEndpoint { get; set; }
    public bool EnableConsoleExporter { get; set; } = false;
}
