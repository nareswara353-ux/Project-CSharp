using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Observability;

public class ObservabilitySettings
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; }

    [Required]
    public string ServiceName { get; set; } = "PortfolioEnterpriseAPI";

    [Required]
    public string ServiceVersion { get; set; } = "1.0.0";

    [Url(ErrorMessage = "Observability:OtlpEndpoint must be a valid URL.")]
    public string? OtlpEndpoint { get; set; }

    public bool EnableConsoleExporter { get; set; }
}
