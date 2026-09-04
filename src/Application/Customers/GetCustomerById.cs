using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Customers;

public record GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public Guid Id { get; init; }
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
            customer.UpdatedAt
        );
    }
}

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string BillingAddress,
    string? ShippingAddress,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
