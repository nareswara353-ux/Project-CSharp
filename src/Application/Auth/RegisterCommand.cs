using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Auth;

public record AuthResponse(string Token, Guid UserId, string Username, string Email, string Role);

public record RegisterCommand : IRequest<Result<AuthResponse>>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
                return Result<AuthResponse>.Failure("Username already exists", "USERNAME_TAKEN");

            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
                return Result<AuthResponse>.Failure("Email already exists", "EMAIL_TAKEN");

            var email = Email.Create(request.Email);
            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User(request.Username, email, passwordHash, "User");

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var token = _jwtTokenService.GenerateToken(user);

            return Result<AuthResponse>.Success(new AuthResponse(
                token, user.Id, user.Username, user.Email.Value, user.Role));
        }
        catch (ArgumentException ex)
        {
            return Result<AuthResponse>.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            return Result<AuthResponse>.Failure($"Registration failed: {ex.Message}", "REGISTER_FAILED");
        }
    }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
    }
}
