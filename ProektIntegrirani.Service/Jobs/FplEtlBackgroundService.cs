using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Jobs;

// Runs the FPL ETL on startup and then every SyncIntervalHours.
public class FplEtlBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly FplApiSettings _settings;
    private readonly ILogger<FplEtlBackgroundService> _logger;

    public FplEtlBackgroundService(IServiceScopeFactory serviceScopeFactory,
        IOptions<FplApiSettings> settings,
        ILogger<FplEtlBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_settings.SyncIntervalHours <= 0)
        {
            _logger.LogInformation("Scheduled FPL ETL is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            // BackgroundService is a singleton; the ETL service and DbContext are scoped.
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IFplEtlService>();
                try
                {
                    _logger.LogInformation("Starting FPL ETL job");
                    await service.SyncAllAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error during FPL ETL job");
                }
            }

            await Task.Delay(TimeSpan.FromHours(_settings.SyncIntervalHours), stoppingToken);
        }
    }
}
