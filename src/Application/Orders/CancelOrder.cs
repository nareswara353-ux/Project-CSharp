using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record CancelOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure($"Order with ID {request.OrderId} not found", "NOT_FOUND");

            order.Cancel();
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
            return Result.Failure($"Failed to cancel order: {ex.Message}", "CANCEL_FAILED");
        }
    }
}

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required");
    }
}
