using Application.Common;
using Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Auth;

public record LoginCommand : IRequest<Result<AuthResponse>>
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var input = request.UsernameOrEmail.Trim();
            var user = input.Contains('@')
                ? await _userRepository.GetByEmailAsync(input, cancellationToken)
                : await _userRepository.GetByUsernameAsync(input, cancellationToken);

            if (user is null)
                return Result<AuthResponse>.Failure("Invalid credentials", "INVALID_CREDENTIALS");

            if (!user.IsActive)
                return Result<AuthResponse>.Failure("User account is inactive", "USER_INACTIVE");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("Invalid credentials", "INVALID_CREDENTIALS");

            user.RecordLogin();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var token = _jwtTokenService.GenerateToken(user);

            return Result<AuthResponse>.Success(new AuthResponse(
                token, user.Id, user.Username, user.Email.Value, user.Role));
        }
        catch (Exception ex)
        {
            return Result<AuthResponse>.Failure($"Login failed: {ex.Message}", "LOGIN_FAILED");
        }
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
