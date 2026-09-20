namespace Application.Common;

public static class ValidationMessageTemplates
{
    public const string Required = "{PropertyName} is required.";
    public const string InvalidEmail = "Invalid email format.";
    public const string InvalidPhone = "Invalid phone number (must be E.164 format).";
    public const string InvalidSku = "SKU must contain only uppercase letters, digits, and dashes.";
    public const string InvalidCurrency = "Currency must be a 3-letter ISO code.";
    public const string GreaterThanZero = "{PropertyName} must be greater than zero.";
    public const string NonNegative = "{PropertyName} cannot be negative.";
    public const string TooShort = "{PropertyName} must be at least {MinLength} characters.";
    public const string TooLong = "{PropertyName} must not exceed {MaxLength} characters.";
}
