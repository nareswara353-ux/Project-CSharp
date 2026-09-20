namespace Application.Common;

public static class GuidExtensions
{
    public static bool IsEmpty(this Guid value) => value == Guid.Empty;

    public static bool IsNotEmpty(this Guid value) => value != Guid.Empty;

    public static string ToShortString(this Guid value)
        => value.ToString("N").Substring(0, 8);

    public static Guid? ToNullableGuid(string? value)
        => Guid.TryParse(value, out var result) ? result : null;
}
