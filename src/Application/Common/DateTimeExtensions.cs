namespace Application.Common;

public static class DateTimeExtensions
{
    public static string ToIso8601(this DateTime dateTime)
        => dateTime.ToUniversalTime().ToString("o");

    public static string? ToIso8601(this DateTime? dateTime)
        => dateTime?.ToUniversalTime().ToString("o");

    public static bool IsExpired(this DateTime dateTime)
        => dateTime < DateTime.UtcNow;

    public static bool IsInFuture(this DateTime dateTime)
        => dateTime > DateTime.UtcNow;

    public static bool IsBetween(this DateTime dateTime, DateTime from, DateTime to)
        => dateTime >= from && dateTime <= to;
}
