using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class UserTests
{
    private readonly Email _email = Email.Create("user@example.com");

    [Fact]
    public void Constructor_ShouldCreateUser_WhenValidParameters()
    {
        var user = new User("johndoe", _email, "hashed-password", "User");

        user.Username.Should().Be("johndoe");
        user.Email.Should().Be(_email);
        user.PasswordHash.Should().Be("hashed-password");
        user.Role.Should().Be("User");
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenUsernameEmpty(string? username)
    {
        Action act = () => new User(username!, _email, "hash");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Username cannot be empty*");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Constructor_ShouldThrowException_WhenUsernameTooShort(string username)
    {
        Action act = () => new User(username, _email, "hash");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 3 and 50*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenUsernameTooLong()
    {
        var username = new string('a', 51);

        Action act = () => new User(username, _email, "hash");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*between 3 and 50*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailNull()
    {
        Action act = () => new User("johndoe", null!, "hash");

        act.Should().Throw<ArgumentNullException>().WithMessage("*email*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPasswordHashEmpty()
    {
        Action act = () => new User("johndoe", _email, "");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }

    [Fact]
    public void UpdateUsername_ShouldChange()
    {
        var user = new User("johndoe", _email, "hash");

        user.UpdateUsername("janedoe");

        user.Username.Should().Be("janedoe");
    }

    [Fact]
    public void UpdateEmail_ShouldChange()
    {
        var user = new User("johndoe", _email, "hash");
        var newEmail = Email.Create("new@example.com");

        user.UpdateEmail(newEmail);

        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdatePasswordHash_ShouldChange()
    {
        var user = new User("johndoe", _email, "old-hash");

        user.UpdatePasswordHash("new-hash");

        user.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public void UpdateRole_ShouldChange()
    {
        var user = new User("johndoe", _email, "hash", "User");

        user.UpdateRole("Admin");

        user.Role.Should().Be("Admin");
    }

    [Fact]
    public void RecordLogin_ShouldSetLastLoginAt()
    {
        var user = new User("johndoe", _email, "hash");

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = new User("johndoe", _email, "hash");
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = new User("johndoe", _email, "hash");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }
}
