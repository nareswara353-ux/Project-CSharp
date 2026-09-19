using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Orders;

public record GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid Id { get; init; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithLinesAsync(request.Id, cancellationToken);

        if (order is null)
            return Result<OrderDto>.Failure($"Order with ID {request.Id} not found", "NOT_FOUND");

        return Result<OrderDto>.Success(MapToDto(order));
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
