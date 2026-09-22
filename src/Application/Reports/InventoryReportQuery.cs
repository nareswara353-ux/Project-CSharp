using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Reports;

public record InventoryReportQuery : IRequest<Result<InventoryReportDto>>
{
    public int LowStockThreshold { get; init; } = 10;
}

public record InventoryReportDto(
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockCount,
    int LowStockCount,
    decimal TotalInventoryValue,
    string Currency,
    IReadOnlyList<LowStockProductDto> LowStockProducts);

public record LowStockProductDto(Guid ProductId, string Name, string Sku, int CurrentStock, int Threshold);

public class InventoryReportQueryHandler : IRequestHandler<InventoryReportQuery, Result<InventoryReportDto>>
{
    private readonly IRepository<Product> _productRepository;

    public InventoryReportQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<InventoryReportDto>> Handle(InventoryReportQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(null, null, cancellationToken);

        var active = products.Where(p => p.IsActive).ToList();
        var outOfStock = active.Where(p => p.StockQuantity == 0).ToList();
        var lowStock = active
            .Where(p => p.StockQuantity > 0 && p.StockQuantity <= request.LowStockThreshold)
            .ToList();

        var totalValue = active.Sum(p => p.Price.Amount * p.StockQuantity);
        var currency = active.FirstOrDefault()?.Price.Currency ?? "USD";

        var lowStockDtos = lowStock
            .Concat(outOfStock)
            .Select(p => new LowStockProductDto(
                p.Id,
                p.Name,
                p.Sku.Value,
                p.StockQuantity,
                request.LowStockThreshold))
            .ToList();

        return Result<InventoryReportDto>.Success(new InventoryReportDto(
            products.Count,
            active.Count,
            outOfStock.Count,
            lowStock.Count,
            totalValue,
            currency,
            lowStockDtos));
    }
}
