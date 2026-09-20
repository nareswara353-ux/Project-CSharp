using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Users;

public record ChangePasswordCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return Result.Failure($"User with ID {request.UserId} not found", "NOT_FOUND");

            if (!user.IsActive)
                return Result.Failure("User account is inactive", "USER_INACTIVE");

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                return Result.Failure("Current password is incorrect", "INVALID_CURRENT_PASSWORD");

            if (request.CurrentPassword == request.NewPassword)
                return Result.Failure("New password must be different from current password", "SAME_PASSWORD");

            var newHash = _passwordHasher.Hash(request.NewPassword);
            user.UpdatePasswordHash(newHash);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to change password: {ex.Message}", "CHANGE_PASSWORD_FAILED");
        }
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit");
    }
}
