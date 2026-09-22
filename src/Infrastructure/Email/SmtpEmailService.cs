using System.Net;
using System.Net.Mail;
using Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("Email disabled, skipping send to {To}", message.To);
            return;
        }

        using var client = BuildClient();
        using var mail = BuildMailMessage(message);

        try
        {
            await client.SendMailAsync(mail, cancellationToken);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", message.To);
            throw;
        }
    }

    public async Task SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            await SendAsync(message, cancellationToken);
        }
    }

    private SmtpClient BuildClient()
    {
        var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        return client;
    }

    private MailMessage BuildMailMessage(EmailMessage message)
    {
        var from = string.IsNullOrWhiteSpace(message.From) ? _settings.FromAddress : message.From;

        var mail = new MailMessage
        {
            From = new MailAddress(from, _settings.FromName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        mail.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.Cc))
            mail.CC.Add(message.Cc);

        if (!string.IsNullOrWhiteSpace(message.Bcc))
            mail.Bcc.Add(message.Bcc);

        return mail;
    }
}
