using Application.Users;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Users;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly UpdateUserRoleCommandHandler _handler;
    private readonly User _user;

    public UpdateUserRoleCommandHandlerTests()
    {
        _handler = new UpdateUserRoleCommandHandler(_userRepo.Object);
        _user = new User("johndoe", Email.Create("john@example.com"), "hash", "User");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("User")]
    [InlineData("admin")]
    [InlineData("MANAGER")]
    public async Task Handle_ShouldReturnSuccess_WhenRoleAllowed(string role)
    {
        var command = new UpdateUserRoleCommand { UserId = _user.Id, Role = role };

        _userRepo.Setup(r => r.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userRepo.Verify(r => r.Update(_user), Times.Once);
        _userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Root")]
    [InlineData("Guest")]
    public async Task Handle_ShouldReturnFailure_WhenRoleNotAllowed(string role)
    {
        var command = new UpdateUserRoleCommand { UserId = _user.Id, Role = role };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ROLE");
        _userRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        var command = new UpdateUserRoleCommand { UserId = Guid.NewGuid(), Role = "Admin" };

        _userRepo.Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }
}
