namespace Application.Common;

public static class CacheKeys
{
    public const string CustomerPrefix = "customer:";
    public const string ProductPrefix = "product:";
    public const string OrderPrefix = "order:";
    public const string UserPrefix = "user:";

    public static string Customer(Guid id) => $"{CustomerPrefix}{id}";
    public static string Product(Guid id) => $"{ProductPrefix}{id}";
    public static string Order(Guid id) => $"{OrderPrefix}{id}";
    public static string User(Guid id) => $"{UserPrefix}{id}";
}
