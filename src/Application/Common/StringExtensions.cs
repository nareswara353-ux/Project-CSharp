namespace Application.Common;

public static class StringExtensions
{
    public static string NormalizeEmail(this string email)
        => email.Trim().ToLowerInvariant();

    public static string NormalizeUsername(this string username)
        => username.Trim().ToLowerInvariant();

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength];
    }

    public static string? NullIfWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
