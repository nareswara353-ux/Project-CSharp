using System.Collections.Concurrent;
using Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Features;

public class InMemoryFeatureFlagService : IFeatureFlagService
{
    private readonly ConcurrentDictionary<string, bool> _flags;
    private readonly ILogger<InMemoryFeatureFlagService> _logger;

    public InMemoryFeatureFlagService(
        IConfiguration configuration,
        ILogger<InMemoryFeatureFlagService> logger)
    {
        _logger = logger;
        _flags = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        var section = configuration.GetSection("FeatureFlags");
        foreach (var child in section.GetChildren())
        {
            if (bool.TryParse(child.Value, out var value))
                _flags[child.Key] = value;
        }

        _logger.LogInformation("Loaded {Count} feature flags from configuration", _flags.Count);
    }

    public Task<bool> IsEnabledAsync(
        string flagName,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            return Task.FromResult(defaultValue);

        var result = _flags.TryGetValue(flagName, out var enabled)
            ? enabled
            : defaultValue;

        return Task.FromResult(result);
    }

    public Task SetAsync(
        string flagName,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            throw new ArgumentException("Flag name cannot be empty", nameof(flagName));

        _flags[flagName] = enabled;
        _logger.LogInformation("Feature flag {FlagName} set to {Enabled}", flagName, enabled);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, bool>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, bool> snapshot =
            new Dictionary<string, bool>(_flags, StringComparer.OrdinalIgnoreCase);

        return Task.FromResult(snapshot);
    }
}
