namespace Application.Common;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    Task SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default);
}

public record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? From = null,
    string? Cc = null,
    string? Bcc = null);

public record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType);
