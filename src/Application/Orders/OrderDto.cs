namespace Application.Orders;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    DateTime OrderDate,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? CancelledAt,
    string? Notes,
    IReadOnlyList<OrderLineDto> Lines,
    decimal TotalAmount,
    string Currency,
    int TotalItemCount);

public record OrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal);
