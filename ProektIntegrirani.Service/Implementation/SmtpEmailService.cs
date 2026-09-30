using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Domain.Dto.Email;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        email.To.Add(new MailboxAddress(message.ToName, message.To));
        email.Subject = message.Subject;
        var builder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainText
        };

        foreach (var attachment in message.Attachments ?? [])
        {
            builder.Attachments.Add(attachment.FileName, attachment.Content,
                ContentType.Parse(attachment.ContentType ?? "application/octet-stream"));
        }

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

            // A local test server (Mailpit) needs no login; Gmail does.
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await smtp.SendAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", message.To);
            throw;
        }
        finally
        {
            if (smtp.IsConnected)
            {
                await smtp.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
