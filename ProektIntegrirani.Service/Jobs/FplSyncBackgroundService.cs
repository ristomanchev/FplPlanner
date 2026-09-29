using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Jobs;

// Runs the FPL ETL on startup and then every SyncIntervalHours.
public class FplSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FplApiSettings _settings;
    private readonly ILogger<FplSyncBackgroundService> _logger;

    public FplSyncBackgroundService(IServiceScopeFactory scopeFactory,
        IOptions<FplApiSettings> settings,
        ILogger<FplSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_settings.SyncIntervalHours <= 0)
        {
            _logger.LogInformation("Scheduled FPL sync is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(_settings.SyncIntervalHours));
        do
        {
            try
            {
                // BackgroundService is a singleton; scoped services (DbContext) need their own scope.
                using var scope = _scopeFactory.CreateScope();
                var etlService = scope.ServiceProvider.GetRequiredService<IFplEtlService>();
                await etlService.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Scheduled FPL sync failed; retrying at the next interval.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
