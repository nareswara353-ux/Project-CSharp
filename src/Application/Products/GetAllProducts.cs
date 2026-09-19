using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Products;

public record GetAllProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IRepository<Product> _productRepository;

    public GetAllProductsQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagination = new PaginationRequest
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = p =>
                (!request.IsActive.HasValue || p.IsActive == request.IsActive.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) ||
                    p.Name.Contains(request.SearchTerm) ||
                    p.Sku.Value.Contains(request.SearchTerm));

            var items = await _productRepository.GetPagedAsync(
                pagination.PageNumber,
                pagination.PageSize,
                predicate,
                q => q.OrderBy(p => p.Name),
                cancellationToken);

            var totalCount = await _productRepository.CountAsync(predicate, cancellationToken);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<ProductDto>(
                dtos,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount);

            return Result<PagedResult<ProductDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ProductDto>>.Failure(
                $"Failed to retrieve products: {ex.Message}",
                "GET_ALL_FAILED");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Sku.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);
    }
}
