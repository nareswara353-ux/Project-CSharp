using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record GetOrdersByCustomerQuery : IRequest<Result<IReadOnlyList<OrderDto>>>
{
    public Guid CustomerId { get; init; }
}

public class GetOrdersByCustomerQueryHandler
    : IRequestHandler<GetOrdersByCustomerQuery, Result<IReadOnlyList<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(
        GetOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            var dtos = orders.Select(MapToDto).ToList();

            return Result<IReadOnlyList<OrderDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrderDto>>.Failure(
                $"Failed to retrieve orders: {ex.Message}",
                "GET_ORDERS_FAILED");
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        var lineDtos = order.Lines.Select(l => new OrderLineDto(
            l.Id,
            l.ProductId,
            l.ProductName,
            l.UnitPrice.Amount,
            l.UnitPrice.Currency,
            l.Quantity.Value,
            l.GetSubtotal().Amount)).ToList();

        var totalAmount = order.Lines.Count > 0 ? order.GetTotalAmount().Amount : 0m;
        var currency = order.Lines.Count > 0 ? order.Lines.First().UnitPrice.Currency : "USD";

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.OrderDate,
            order.ConfirmedAt,
            order.ShippedAt,
            order.CancelledAt,
            order.Notes,
            lineDtos,
            totalAmount,
            currency,
            order.GetTotalItemCount());
    }
}

public class GetOrdersByCustomerQueryValidator : AbstractValidator<GetOrdersByCustomerQuery>
{
    public GetOrdersByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer ID is required");
    }
}
