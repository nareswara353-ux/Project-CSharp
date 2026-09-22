using System.Diagnostics;

namespace Application.Common.Diagnostics;

public static class ActivitySources
{
    public const string ApplicationName = "PortfolioEnterprise.Application";

    public static readonly ActivitySource Application = new(ApplicationName, "1.0.0");
}
