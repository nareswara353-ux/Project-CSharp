using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Customers;

public record GetAllCustomersQuery : IRequest<Result<PagedResult<CustomerDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly IRepository<Customer> _customerRepository;

    public GetAllCustomersQueryHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagination = new PaginationRequest
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            System.Linq.Expressions.Expression<Func<Customer, bool>>? predicate = c =>
                (!request.IsActive.HasValue || c.IsActive == request.IsActive.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) ||
                    c.FirstName.Contains(request.SearchTerm) ||
                    c.LastName.Contains(request.SearchTerm) ||
                    c.Email.Value.Contains(request.SearchTerm));

            var items = await _customerRepository.GetPagedAsync(
                pagination.PageNumber,
                pagination.PageSize,
                predicate,
                q => q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
                cancellationToken);

            var totalCount = await _customerRepository.CountAsync(predicate, cancellationToken);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<CustomerDto>(
                dtos,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount);

            return Result<PagedResult<CustomerDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<CustomerDto>>.Failure(
                $"Failed to retrieve customers: {ex.Message}",
                "GET_ALL_FAILED");
        }
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.BillingAddress.ToString(),
            customer.ShippingAddress?.ToString(),
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt
        );
    }
}
