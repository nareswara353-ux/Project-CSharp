using Application.Common;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record AddOrderLineCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public int Quantity { get; init; }
}

public class AddOrderLineCommandHandler : IRequestHandler<AddOrderLineCommand, Result>
{
    private readonly IOrderRepository _orderRepository;

    public AddOrderLineCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(AddOrderLineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetWithLinesAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure($"Order with ID {request.OrderId} not found", "NOT_FOUND");

            var price = new Money(request.UnitPrice, request.Currency);
            var quantity = new Quantity(request.Quantity);

            order.AddLine(request.ProductId, request.ProductName, price, quantity);

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure($"Business rule violation: {ex.Message}", "BUSINESS_RULE_VIOLATION");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to add order line: {ex.Message}", "ADD_LINE_FAILED");
        }
    }
}

public class AddOrderLineCommandValidator : AbstractValidator<AddOrderLineCommand>
{
    public AddOrderLineCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product name is required");
        RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Currency must be a 3-letter ISO code");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive");
    }
}
