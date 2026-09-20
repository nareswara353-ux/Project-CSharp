using Application.Auth;
using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly LoginCommandHandler _handler;
    private readonly User _user;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepo.Object, _hasher.Object, _jwt.Object);
        _user = new User("johndoe", Email.Create("john@example.com"), "hashed-pwd", "User");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenLoginWithUsername()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("Password123", "hashed-pwd")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(_user)).Returns("jwt-token");

        var result = await _handler.Handle(
            new LoginCommand { UsernameOrEmail = "johndoe", Password = "Password123" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        _userRepo.Verify(r => r.Update(_user), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenLoginWithEmail()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("Password123", "hashed-pwd")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(_user)).Returns("jwt-token");

        var result = await _handler.Handle(
            new LoginCommand { UsernameOrEmail = "john@example.com", Password = "Password123" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new LoginCommand { UsernameOrEmail = "ghost", Password = "Password123" },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordWrong()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("WrongPass", "hashed-pwd")).Returns(false);

        var result = await _handler.Handle(
            new LoginCommand { UsernameOrEmail = "johndoe", Password = "WrongPass" },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserInactive()
    {
        _user.Deactivate();
        _userRepo.Setup(r => r.GetByUsernameAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);

        var result = await _handler.Handle(
            new LoginCommand { UsernameOrEmail = "johndoe", Password = "Password123" },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_INACTIVE");
    }
}
