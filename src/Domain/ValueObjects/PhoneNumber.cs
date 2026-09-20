using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; } = null!;

    private PhoneNumber() { }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        var normalized = Normalize(phoneNumber);

        if (!IsValidE164(normalized))
            throw new ArgumentException(
                "Phone number must be in E.164 format (e.g., +6281234567890)",
                nameof(phoneNumber));

        return new PhoneNumber(normalized);
    }

    public static bool TryCreate(string? phoneNumber, out PhoneNumber? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var normalized = Normalize(phoneNumber);

        if (!IsValidE164(normalized))
            return false;

        result = new PhoneNumber(normalized);
        return true;
    }

    private static string Normalize(string input)
    {
        var cleaned = CleanupRegex().Replace(input.Trim(), string.Empty);

        if (!cleaned.StartsWith('+'))
            cleaned = "+" + cleaned;

        return cleaned;
    }

    private static bool IsValidE164(string value)
    {
        if (value.Length < 8 || value.Length > 16)
            return false;

        return E164Regex().IsMatch(value);
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"[\s\-\(\)\.]+")]
    private static partial Regex CleanupRegex();

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex E164Regex();
}
