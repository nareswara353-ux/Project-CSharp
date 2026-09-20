namespace Application.Common;

public static class ValidationRegexPatterns
{
    public const string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public const string PhoneE164 = @"^\+[1-9]\d{7,14}$";
    public const string Sku = @"^[A-Z0-9]+(-[A-Z0-9]+)*$";
    public const string Username = @"^[a-zA-Z0-9_]{3,50}$";
    public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
    public const string PostalCode = @"^[A-Z0-9][A-Z0-9\s-]{2,19}$";
    public const string CurrencyCode = @"^[A-Z]{3}$";
}
