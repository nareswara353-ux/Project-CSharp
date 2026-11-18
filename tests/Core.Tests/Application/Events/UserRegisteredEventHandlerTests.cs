using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class UserRegisteredEventHandlerTests
{
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateRenderer> _renderer = new();
    private readonly Mock<ILogger<UserRegisteredEventHandler>> _logger = new();
    private readonly UserRegisteredEventHandler _handler;

    public UserRegisteredEventHandlerTests()
    {
        _handler = new UserRegisteredEventHandler(
            _emailService.Object,
            _renderer.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldRenderTemplateAndSendEmail_WhenEventReceived()
    {
        var userId = Guid.NewGuid();
        var domainEvent = new UserRegisteredEvent(userId, "johndoe", "john@example.com");
        var notification = new DomainEventNotification<UserRegisteredEvent>(domainEvent);

        var expectedBody = "<h1>Welcome johndoe!</h1>";
        _renderer.Setup(r => r.RenderAsync(EmailTemplateNames.UserWelcome, It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBody);

        await _handler.Handle(notification, CancellationToken.None);

        _renderer.Verify(r => r.RenderAsync(
            EmailTemplateNames.UserWelcome,
            It.Is<object>(o => 
                o.GetType().GetProperty("Username")!.GetValue(o)!.ToString() == "johndoe" &&
                o.GetType().GetProperty("Email")!.GetValue(o)!.ToString() == "john@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailService.Verify(e => e.SendAsync(
            It.Is<EmailMessage>(m => 
                m.To == "john@example.com" && 
                m.Subject == EmailSubjects.UserWelcome && 
                m.Body == expectedBody),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
