using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProektIntegrirani.Domain.Dto.Email;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Jobs;

// Reads queued e-mails and sends them one by one, so HTTP requests never wait for SMTP.
public class EmailBackgroundService : BackgroundService
{
    private readonly Channel<EmailMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(Channel<EmailMessage> channel, IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmailAsync(message, stoppingToken);
                _logger.LogInformation("Sent email '{Subject}' to {To}", message.Subject, message.To);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to send queued email to {To}", message.To);
            }
        }
    }
}
