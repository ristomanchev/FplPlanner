using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

// MailKit instead of System.Net.Mail.SmtpClient, which Microsoft marks as not recommended for new code.
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
            _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

        if (!string.IsNullOrEmpty(_settings.UserName))
        {
            await client.AuthenticateAsync(_settings.UserName, _settings.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
