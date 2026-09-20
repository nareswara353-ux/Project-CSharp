namespace Domain.Common;

public static class Guard
{
    public static void AgainstNull(object? value, string parameterName)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative.", parameterName);
    }

    public static void AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative.", parameterName);
    }

    public static void AgainstEmptyGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
    }

    public static void AgainstOutOfRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be between {min} and {max}.");
    }

    public static void AgainstTooLong(string? value, int maxLength, string parameterName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException(
                $"{parameterName} cannot exceed {maxLength} characters.",
                parameterName);
    }
}
