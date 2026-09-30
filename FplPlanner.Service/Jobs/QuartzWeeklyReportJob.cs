using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FplPlanner.Domain.Configuration;
using FplPlanner.Service.Interface;
using Quartz;

namespace FplPlanner.Service.Jobs;

// Runs every hour (cron in Program.cs). When the next deadline is within SendHoursBeforeDeadline,
// the weekly reports are queued; Manager.LastReportedGameweek makes sure each manager gets one per gameweek.
[DisallowConcurrentExecution]
public class QuartzWeeklyReportJob : IJob
{
    private readonly IGameweekService _gameweekService;
    private readonly IWeeklyReportService _weeklyReportService;
    private readonly EmailSettings _settings;
    private readonly ILogger<QuartzWeeklyReportJob> _logger;

    public QuartzWeeklyReportJob(IGameweekService gameweekService,
        IWeeklyReportService weeklyReportService,
        IOptions<EmailSettings> settings,
        ILogger<QuartzWeeklyReportJob> logger)
    {
        _gameweekService = gameweekService;
        _weeklyReportService = weeklyReportService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (_settings.SendHoursBeforeDeadline <= 0)
        {
            return;
        }

        var next = await _gameweekService.GetNextAsync();
        var untilDeadline = next.Deadline - DateTime.UtcNow;

        // Only inside the window before the deadline: too early the data may still change, after it the advice is useless.
        if (untilDeadline <= TimeSpan.Zero || untilDeadline > TimeSpan.FromHours(_settings.SendHoursBeforeDeadline))
        {
            _logger.LogDebug("Not in the report window for gameweek {Gameweek}.", next.Number);
            return;
        }

        var sent = await _weeklyReportService.SendDueReportsAsync(context.CancellationToken);
        _logger.LogInformation("Weekly report job queued {Count} reports for gameweek {Gameweek}.", sent, next.Number);
    }
}
