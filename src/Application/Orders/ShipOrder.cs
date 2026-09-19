using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record ShipOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
}

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;

    public ShipOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure($"Order with ID {request.OrderId} not found", "NOT_FOUND");

            order.Ship();
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure($"Business rule violation: {ex.Message}", "BUSINESS_RULE_VIOLATION");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to ship order: {ex.Message}", "SHIP_FAILED");
        }
    }
}

public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required");
    }
}
