using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class User : Entity
{
    public string Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = "User";
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User()
    {
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public User(string username, Email email, string passwordHash, string role = "User") : base()
    {
        SetUsername(username);
        Email = email ?? throw new ArgumentNullException(nameof(email));
        SetPasswordHash(passwordHash);
        SetRole(role);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateUsername(string username) => SetUsername(username);
    public void UpdateEmail(Email email) => Email = email ?? throw new ArgumentNullException(nameof(email));
    public void UpdatePasswordHash(string passwordHash) => SetPasswordHash(passwordHash);
    public void UpdateRole(string role) => SetRole(role);

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters", nameof(username));

        Username = username.Trim();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    private void SetRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));

        Role = role.Trim();
    }
}
