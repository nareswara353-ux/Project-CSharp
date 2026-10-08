using Application.Common;
using Application.Common.Behaviors;
using Domain.Entities;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Customers;

public record DeleteCustomerCommand : IRequest<Result>, IAuditableRequest
{
    public Guid Id { get; init; }

    public string AuditAction => "CustomerDeactivated";
    public string AuditEntityType => nameof(Customer);
    public string? AuditEntityId => Id.ToString();
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly IRepository<Customer> _customerRepository;

    public DeleteCustomerCommandHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null)
                return Result.Failure($"Customer with ID {request.Id} not found", "NOT_FOUND");

            customer.Deactivate();
            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete customer: {ex.Message}", "DELETE_FAILED");
        }
    }
}

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}
