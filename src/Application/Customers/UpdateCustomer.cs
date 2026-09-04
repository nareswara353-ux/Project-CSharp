using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using FluentValidation;

namespace Application.Customers;

public record UpdateCustomerCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IRepository<Customer> _customerRepository;

    public UpdateCustomerCommandHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null)
                return Result.Failure($"Customer with ID {request.Id} not found", "NOT_FOUND");

            // Update domain aggregates using domain methods
            customer.UpdateName(request.FirstName, request.LastName);
            customer.UpdateEmail(Email.Create(request.Email));
            
            var newAddress = new Address(
                request.Street,
                request.City,
                request.State,
                request.PostalCode,
                request.Country
            );
            customer.UpdateBillingAddress(newAddress);

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update customer: {ex.Message}", "UPDATE_FAILED");
        }
    }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");

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
