using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Orders;

public record GetOrdersByStatusQuery : IRequest<Result<IReadOnlyList<OrderDto>>>
{
    public OrderStatus Status { get; init; }
}

public class GetOrdersByStatusQueryHandler
    : IRequestHandler<GetOrdersByStatusQuery, Result<IReadOnlyList<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(
        GetOrdersByStatusQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetByStatusAsync(
                request.Status,
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

public class GetOrdersByStatusQueryValidator : AbstractValidator<GetOrdersByStatusQuery>
{
    public GetOrdersByStatusQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status");
    }
}
