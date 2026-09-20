using Application.Auth;
using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepo.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRegistrationValid()
    {
        var command = new RegisterCommand
        {
            Username = "johndoe",
            Email = "john@example.com",
            Password = "Password123"
        };

        _userRepo.Setup(r => r.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepo.Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash(command.Password)).Returns("hashed-pwd");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.Username.Should().Be("johndoe");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameTaken()
    {
        var command = new RegisterCommand
        {
            Username = "johndoe",
            Email = "john@example.com",
            Password = "Password123"
        };

        _userRepo.Setup(r => r.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USERNAME_TAKEN");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailTaken()
    {
        var command = new RegisterCommand
        {
            Username = "johndoe",
            Email = "john@example.com",
            Password = "Password123"
        };

        _userRepo.Setup(r => r.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepo.Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_TAKEN");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailInvalid()
    {
        var command = new RegisterCommand
        {
            Username = "johndoe",
            Email = "invalid-email",
            Password = "Password123"
        };

        _userRepo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}
