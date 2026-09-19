using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record ConfirmOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetWithLinesAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure($"Order with ID {request.OrderId} not found", "NOT_FOUND");

            order.Confirm();
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
            return Result.Failure($"Failed to confirm order: {ex.Message}", "CONFIRM_FAILED");
        }
    }
}

public class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
