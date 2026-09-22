using Application.Common;
using Application.Common.Behaviors;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Customers;

public record GetCustomerByIdQuery : IRequest<Result<CustomerDto>>, ICacheableQuery
{
    public Guid Id { get; init; }

    public string CacheKey => CacheKeys.Customer(Id);
    public TimeSpan? CacheExpiration => CacheDurations.Medium;
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IRepository<Customer> _customerRepository;

    public GetCustomerByIdQueryHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
            return Result<CustomerDto>.Failure($"Customer with ID {request.Id} not found", "NOT_FOUND");

        return Result<CustomerDto>.Success(MapToDto(customer));
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
            customer.UpdatedAt);
    }
}
