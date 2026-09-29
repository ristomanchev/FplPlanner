using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Jobs;

// Every hour: if the next deadline is within SendHoursBeforeDeadline, e-mail the weekly reports.
// Manager.LastReportedGameweek makes this idempotent, so each manager gets one report per gameweek.
public class WeeklyReportBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmailSettings _settings;
    private readonly ILogger<WeeklyReportBackgroundService> _logger;

    public WeeklyReportBackgroundService(IServiceScopeFactory scopeFactory,
        IOptions<EmailSettings> settings,
        ILogger<WeeklyReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_settings.SendHoursBeforeDeadline <= 0)
        {
            _logger.LogInformation("Scheduled weekly reports are disabled.");
            return;
        }

        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var gameweekService = scope.ServiceProvider.GetRequiredService<IGameweekService>();
                var next = await gameweekService.GetNextAsync();

                if (next.Deadline - DateTime.UtcNow > TimeSpan.FromHours(_settings.SendHoursBeforeDeadline))
                {
                    continue;
                }

                var reportService = scope.ServiceProvider.GetRequiredService<IWeeklyReportService>();
                var sent = await reportService.SendDueReportsAsync(stoppingToken);
                if (sent > 0)
                {
                    _logger.LogInformation("Sent {Count} weekly reports for gameweek {Gameweek}.", sent, next.Number);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Weekly report job failed; retrying in an hour.");
            }
        }
    }
}
