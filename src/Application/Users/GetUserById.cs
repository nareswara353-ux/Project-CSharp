using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Users;

public record GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid Id { get; init; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result<UserDto>.Failure($"User with ID {request.Id} not found", "NOT_FOUND");

        return Result<UserDto>.Success(MapToDto(user));
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Username,
        user.Email.Value,
        user.Role,
        user.IsActive,
        user.CreatedAt,
        user.LastLoginAt);
}
