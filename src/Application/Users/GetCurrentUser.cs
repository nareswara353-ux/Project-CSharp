using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Users;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result<UserDto>.Failure("Current user not found", "NOT_FOUND");

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt));
    }
}
