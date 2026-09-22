using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class UserRegisteredEventHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IEmailService emailService,
        IEmailTemplateRenderer renderer,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _emailService = emailService;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserRegisteredEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "Handling UserRegisteredEvent for user {UserId} ({Username})",
            evt.UserId,
            evt.Username);

        var body = await _renderer.RenderAsync(
            EmailTemplateNames.UserWelcome,
            new { evt.Username, evt.Email },
            cancellationToken);

        var message = new EmailMessage(
            To: evt.Email,
            Subject: EmailSubjects.UserWelcome,
            Body: body);

        await _emailService.SendAsync(message, cancellationToken);
    }
}
