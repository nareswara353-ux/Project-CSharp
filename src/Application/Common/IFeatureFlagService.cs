namespace Application.Common;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagName, bool defaultValue = false, CancellationToken cancellationToken = default);

    Task SetAsync(string flagName, bool enabled, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default);
}

public static class FeatureFlags
{
    public const string EnableNewCheckout = "EnableNewCheckout";
    public const string EnableProductReviews = "EnableProductReviews";
    public const string EnableBulkOrderImport = "EnableBulkOrderImport";
    public const string EnableCustomerLoyalty = "EnableCustomerLoyalty";
    public const string EnableRealtimeNotifications = "EnableRealtimeNotifications";
    public const string MaintenanceMode = "MaintenanceMode";
}
