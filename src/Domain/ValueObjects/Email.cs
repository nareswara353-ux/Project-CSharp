using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email() { } // For EF Core

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        // Trim and normalize
        var normalized = email.Trim().ToLowerInvariant();

        // Validate format using regex
        if (!EmailRegex().IsMatch(normalized))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    public static explicit operator Email(string value) => Create(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
