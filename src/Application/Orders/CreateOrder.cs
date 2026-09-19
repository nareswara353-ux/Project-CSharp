using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record OrderLineRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public int Quantity { get; init; }
}

public record CreateOrderCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<OrderLineRequest> Lines { get; init; } = Array.Empty<OrderLineRequest>();
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Customer> _customerRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IRepository<Customer> customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result<Guid>.Failure($"Customer with ID {request.CustomerId} not found", "CUSTOMER_NOT_FOUND");

            var order = new Order(request.CustomerId, request.Notes);

            foreach (var line in request.Lines)
            {
                var price = new Money(line.UnitPrice, line.Currency);
                var quantity = new Quantity(line.Quantity);
                order.AddLine(line.ProductId, line.ProductName, price, quantity);
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(order.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure($"Business rule violation: {ex.Message}", "BUSINESS_RULE_VIOLATION");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create order: {ex.Message}", "CREATE_FAILED");
        }
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Order must contain at least one line item");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty().WithMessage("Product ID is required");
            line.RuleFor(l => l.ProductName).NotEmpty().WithMessage("Product name is required");
            line.RuleFor(l => l.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero");
            line.RuleFor(l => l.Currency).NotEmpty().Length(3).WithMessage("Currency must be a 3-letter ISO code");
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity must be positive");
        });
    }
}
