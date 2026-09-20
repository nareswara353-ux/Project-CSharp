using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Users;

public record UpdateUserRoleCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = string.Empty;
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager",
        "User"
    };

    private readonly IUserRepository _userRepository;

    public UpdateUserRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var normalizedRole = request.Role.Trim();
            if (!AllowedRoles.Contains(normalizedRole))
                return Result.Failure($"Role '{request.Role}' is not allowed", "INVALID_ROLE");

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return Result.Failure($"User with ID {request.UserId} not found", "NOT_FOUND");

            user.UpdateRole(normalizedRole);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update role: {ex.Message}", "UPDATE_ROLE_FAILED");
        }
    }
}

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(50).WithMessage("Role must not exceed 50 characters");
    }
}
