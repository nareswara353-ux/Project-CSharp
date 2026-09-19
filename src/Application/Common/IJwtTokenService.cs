using Domain.Entities;

namespace Application.Common;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
