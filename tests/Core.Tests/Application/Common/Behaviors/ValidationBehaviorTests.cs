using Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    public record TestRequest(string Data) : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
        var nextCalled = false;

        var response = await behavior.Handle(
            new TestRequest("test"),
            _ => { nextCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        response.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator.Object });
        var nextCalled = false;

        var response = await behavior.Handle(
            new TestRequest("valid"),
            _ => { nextCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        response.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var failures = new List<ValidationFailure>
        {
            new("Data", "Data is required")
        };

        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator.Object });
        var nextCalled = false;

        Func<Task> act = async () => await behavior.Handle(
            new TestRequest(""),
            _ => { nextCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage == "Data is required"));

        nextCalled.Should().BeFalse();
    }
}
