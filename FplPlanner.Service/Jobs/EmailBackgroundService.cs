using System.Threading.Channels;
using FplPlanner.Service.Implementation;
using FplPlanner.Service.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FplPlanner.Service.Jobs;

// Reads queued e-mails and sends them one by one, so HTTP requests never wait for SMTP.
// A failed send is retried; the follow-up (e.g. marking a report as sent) runs only after success.
public class EmailBackgroundService : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)];

    private readonly Channel<QueuedEmail> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(Channel<QueuedEmail> channel, IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var queued in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (await TrySendAsync(queued, stoppingToken) && queued.OnSent != null)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await queued.OnSent(scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Email to {To} was sent, but its follow-up failed.", queued.Message.To);
                }
            }
        }
    }

    private async Task<bool> TrySendAsync(QueuedEmail queued, CancellationToken stoppingToken)
    {
        var message = queued.Message;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmailAsync(message, stoppingToken);
                _logger.LogInformation("Sent email '{Subject}' to {To}", message.Subject, message.To);
                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt > RetryDelays.Length)
                {
                    _logger.LogError(ex, "Giving up on email to {To} after {Attempts} attempts.", message.To, attempt);
                    return false;
                }

                _logger.LogWarning("Sending email to {To} failed (attempt {Attempt}): {Error}. Retrying.",
                    message.To, attempt, ex.Message);
                await Task.Delay(RetryDelays[attempt - 1], stoppingToken);
            }
        }
    }
}
