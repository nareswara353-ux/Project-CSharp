using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class CustomerDeactivatedEventHandler
    : INotificationHandler<DomainEventNotification<CustomerDeactivatedEvent>>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<CustomerDeactivatedEventHandler> _logger;

    public CustomerDeactivatedEventHandler(
        IEmailService emailService,
        IEmailTemplateRenderer renderer,
        ILogger<CustomerDeactivatedEventHandler> logger)
    {
        _emailService = emailService;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<CustomerDeactivatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "Handling CustomerDeactivatedEvent for customer {CustomerId}",
            evt.CustomerId);

        var body = await _renderer.RenderAsync(
            EmailTemplateNames.CustomerDeactivated,
            new { FullName = evt.Email, evt.Email },
            cancellationToken);

        var message = new EmailMessage(
            To: evt.Email,
            Subject: EmailSubjects.CustomerDeactivated,
            Body: body);

        await _emailService.SendAsync(message, cancellationToken);
    }
}
