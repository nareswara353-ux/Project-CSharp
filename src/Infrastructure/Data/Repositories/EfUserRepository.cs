using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class EfUserRepository : EfRepository<User>, IUserRepository
{
    public EfUserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(
            u => u.Username.ToLower() == normalized, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(
            u => u.Email.Value == normalized, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(u => u.Username.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(u => u.Email.Value == normalized, cancellationToken);
    }
}
