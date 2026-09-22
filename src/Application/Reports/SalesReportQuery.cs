using Application.Common;
using Domain.Enums;
using Domain.Repositories;
using MediatR;

namespace Application.Reports;

public record SalesReportQuery : IRequest<Result<SalesReportDto>>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}

public record SalesReportDto(
    DateTime From,
    DateTime To,
    int TotalOrders,
    int ConfirmedOrders,
    int ShippedOrders,
    int CancelledOrders,
    decimal TotalRevenue,
    string Currency,
    IReadOnlyList<DailySalesDto> DailyBreakdown);

public record DailySalesDto(DateTime Date, int Orders, decimal Revenue);

public class SalesReportQueryHandler : IRequestHandler<SalesReportQuery, Result<SalesReportDto>>
{
    private readonly IOrderRepository _orderRepository;

    public SalesReportQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<SalesReportDto>> Handle(SalesReportQuery request, CancellationToken cancellationToken)
    {
        if (request.From > request.To)
            return Result<SalesReportDto>.Failure("From date must be before To date", "INVALID_RANGE");

        var allOrders = await _orderRepository.GetAllAsync(
            o => o.OrderDate >= request.From && o.OrderDate <= request.To,
            q => q.OrderBy(o => o.OrderDate),
            cancellationToken);

        var confirmed = allOrders.Count(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Shipped);
        var shipped = allOrders.Count(o => o.Status == OrderStatus.Shipped);
        var cancelled = allOrders.Count(o => o.Status == OrderStatus.Cancelled);

        var revenue = allOrders
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Shipped)
            .Where(o => o.Lines.Count > 0)
            .Sum(o => o.GetTotalAmount().Amount);

        var currency = allOrders.FirstOrDefault(o => o.Lines.Count > 0)?.Lines.FirstOrDefault()?.UnitPrice.Currency ?? "USD";

        var daily = allOrders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailySalesDto(
                g.Key,
                g.Count(),
                g.Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Shipped)
                 .Where(o => o.Lines.Count > 0)
                 .Sum(o => o.GetTotalAmount().Amount)))
            .OrderBy(d => d.Date)
            .ToList();

        return Result<SalesReportDto>.Success(new SalesReportDto(
            request.From,
            request.To,
            allOrders.Count,
            confirmed,
            shipped,
            cancelled,
            revenue,
            currency,
            daily));
    }
}
