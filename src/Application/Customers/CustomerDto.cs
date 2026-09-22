namespace Application.Customers;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string BillingAddress,
    string? ShippingAddress,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
