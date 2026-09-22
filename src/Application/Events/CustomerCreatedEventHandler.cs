using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class CustomerCreatedEventHandler
    : INotificationHandler<DomainEventNotification<CustomerCreatedEvent>>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(
        IEmailService emailService,
        IEmailTemplateRenderer renderer,
        ILogger<CustomerCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<CustomerCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "Handling CustomerCreatedEvent for customer {CustomerId}",
            evt.CustomerId);

        var body = await _renderer.RenderAsync(
            EmailTemplateNames.WelcomeCustomer,
            new { evt.FullName, evt.Email },
            cancellationToken);

        var message = new EmailMessage(
            To: evt.Email,
            Subject: EmailSubjects.WelcomeCustomer,
            Body: body);

        await _emailService.SendAsync(message, cancellationToken);
    }
}
