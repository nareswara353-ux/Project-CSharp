using Application.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[NoOpEmail] To={To} Subject={Subject}",
            message.To,
            message.Subject);

        return Task.CompletedTask;
    }

    public Task SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
            _logger.LogInformation("[NoOpEmail] To={To} Subject={Subject}", message.To, message.Subject);

        return Task.CompletedTask;
    }
}
