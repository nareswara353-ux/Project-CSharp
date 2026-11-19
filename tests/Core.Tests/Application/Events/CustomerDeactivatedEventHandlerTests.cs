using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class CustomerDeactivatedEventHandlerTests
{
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateRenderer> _renderer = new();
    private readonly Mock<ILogger<CustomerDeactivatedEventHandler>> _logger = new();
    private readonly CustomerDeactivatedEventHandler _handler;

    public CustomerDeactivatedEventHandlerTests()
    {
        _handler = new CustomerDeactivatedEventHandler(
            _emailService.Object,
            _renderer.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldRenderTemplateAndSendEmail_WhenEventReceived()
    {
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerDeactivatedEvent(customerId, "jane@example.com");
        var notification = new DomainEventNotification<CustomerDeactivatedEvent>(domainEvent);

        var expectedBody = "<h1>Account Deactivated</h1>";
        _renderer.Setup(r => r.RenderAsync(EmailTemplateNames.CustomerDeactivated, It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBody);

        await _handler.Handle(notification, CancellationToken.None);

        _renderer.Verify(r => r.RenderAsync(
            EmailTemplateNames.CustomerDeactivated,
            It.Is<object>(o => o.GetType().GetProperty("Email")!.GetValue(o)!.ToString() == "jane@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailService.Verify(e => e.SendAsync(
            It.Is<EmailMessage>(m => 
                m.To == "jane@example.com" && 
                m.Subject == EmailSubjects.CustomerDeactivated && 
                m.Body == expectedBody),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
