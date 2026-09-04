using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using FluentValidation;

namespace Application.Customers;

public record CreateCustomerCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IRepository<Customer> _customerRepository;

    public CreateCustomerCommandHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = Email.Create(request.Email);
            var address = new Address(request.Street, request.City, request.State, request.PostalCode, request.Country);
            var customer = new Customer(request.FirstName, request.LastName, email, address);

            await _customerRepository.AddAsync(customer, cancellationToken);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(customer.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create customer: {ex.Message}", "CREATE_FAILED");
        }
    }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
        RuleFor(x => x.State).NotEmpty().WithMessage("State is required");
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage("Postal code is required");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required");
    }
}
