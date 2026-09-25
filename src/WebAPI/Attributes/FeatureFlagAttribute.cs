using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class FeatureFlagAttribute : TypeFilterAttribute
{
    public string FlagName { get; }

    public FeatureFlagAttribute(string flagName)
        : base(typeof(FeatureFlagAuthorizationFilter))
    {
        if (string.IsNullOrWhiteSpace(flagName))
            throw new ArgumentException("Flag name cannot be empty", nameof(flagName));

        FlagName = flagName;
        Arguments = new object[] { flagName };
    }
}
