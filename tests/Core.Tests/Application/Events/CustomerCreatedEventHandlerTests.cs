using Application.Common;
using Application.Events;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Events;

public class CustomerCreatedEventHandlerTests
{
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateRenderer> _renderer = new();
    private readonly Mock<ILogger<CustomerCreatedEventHandler>> _logger = new();
    private readonly CustomerCreatedEventHandler _handler;

    public CustomerCreatedEventHandlerTests()
    {
        _handler = new CustomerCreatedEventHandler(
            _emailService.Object,
            _renderer.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldRenderTemplateAndSendEmail_WhenEventReceived()
    {
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerCreatedEvent(customerId, "john@example.com", "John Doe");
        var notification = new DomainEventNotification<CustomerCreatedEvent>(domainEvent);

        var expectedBody = "<h1>Welcome John Doe</h1>";
        _renderer.Setup(r => r.RenderAsync(EmailTemplateNames.WelcomeCustomer, It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBody);

        await _handler.Handle(notification, CancellationToken.None);

        _renderer.Verify(r => r.RenderAsync(
            EmailTemplateNames.WelcomeCustomer,
            It.Is<object>(o => o.GetType().GetProperty("FullName")!.GetValue(o)!.ToString() == "John Doe"),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailService.Verify(e => e.SendAsync(
            It.Is<EmailMessage>(m => 
                m.To == "john@example.com" && 
                m.Subject == EmailSubjects.WelcomeCustomer && 
                m.Body == expectedBody),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
