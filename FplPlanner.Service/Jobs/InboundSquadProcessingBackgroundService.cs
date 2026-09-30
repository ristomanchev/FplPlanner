using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FplPlanner.Service.Implementation;

namespace FplPlanner.Service.Jobs;

// Polls for Pending inbound squads and hands them to InboundSquadEntryProcessor.
public class InboundSquadProcessingBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InboundSquadProcessingBackgroundService> _logger;

    public InboundSquadProcessingBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<InboundSquadProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // BackgroundService is a singleton; the processor and DbContext are scoped.
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<InboundSquadEntryProcessor>();
                await processor.ProcessPendingEntriesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Inbound squad processing failed; retrying.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
