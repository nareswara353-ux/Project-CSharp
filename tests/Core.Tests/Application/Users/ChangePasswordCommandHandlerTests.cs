using Application.Common;
using Application.Users;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Users;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly ChangePasswordCommandHandler _handler;
    private readonly User _user;

    public ChangePasswordCommandHandlerTests()
    {
        _handler = new ChangePasswordCommandHandler(_userRepo.Object, _hasher.Object);
        _user = new User("johndoe", Email.Create("john@example.com"), "old-hash", "User");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCurrentPasswordValid()
    {
        var command = new ChangePasswordCommand
        {
            UserId = _user.Id,
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456"
        };

        _userRepo.Setup(r => r.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("OldPassword123", "old-hash")).Returns(true);
        _hasher.Setup(h => h.Hash("NewPassword456")).Returns("new-hash");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _user.PasswordHash.Should().Be("new-hash");
        _userRepo.Verify(r => r.Update(_user), Times.Once);
        _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCurrentPasswordWrong()
    {
        var command = new ChangePasswordCommand
        {
            UserId = _user.Id,
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword456"
        };

        _userRepo.Setup(r => r.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("WrongPassword", "old-hash")).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CURRENT_PASSWORD");
        _userRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        var command = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456"
        };

        _userRepo.Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewPasswordSameAsCurrent()
    {
        var command = new ChangePasswordCommand
        {
            UserId = _user.Id,
            CurrentPassword = "SamePassword123",
            NewPassword = "SamePassword123"
        };

        _userRepo.Setup(r => r.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("SamePassword123", "old-hash")).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("SAME_PASSWORD");
    }
}
