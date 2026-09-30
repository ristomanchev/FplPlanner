using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Messages;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class FplEtlService : IFplEtlService
{
    public const string JobName = "FplSync";

    private readonly IFplApiClient _fplApiClient;
    private readonly IFplDataRepository _fplDataRepository;
    private readonly IRepository<EtlSyncLog> _etlSyncLogRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly ILogger<FplEtlService> _logger;

    public FplEtlService(IFplApiClient fplApiClient,
        IFplDataRepository fplDataRepository,
        IRepository<EtlSyncLog> etlSyncLogRepository,
        IMessagePublisher messagePublisher,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        ILogger<FplEtlService> logger)
    {
        _fplApiClient = fplApiClient;
        _fplDataRepository = fplDataRepository;
        _etlSyncLogRepository = etlSyncLogRepository;
        _messagePublisher = messagePublisher;
        _rabbitMqSettings = rabbitMqSettings.Value;
        _logger = logger;
    }

    public async Task<EtlSyncLog> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var syncLog = new EtlSyncLog
        {
            JobName = JobName,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting FPL ETL");

            // Extract
            var bootstrap = await _fplApiClient.GetBootstrapAsync(cancellationToken);
            var fplFixtures = await _fplApiClient.GetFixturesAsync(cancellationToken);

            // Transform
            var clubs = bootstrap.Teams.Select(FplTransformations.ToClub).ToList();
            var clubIds = clubs.Select(c => c.Id).ToHashSet();
            var gameweeks = bootstrap.Events.Select(FplTransformations.ToGameweek).ToList();
            var players = bootstrap.Elements
                .Where(e => e.ElementType is >= 1 and <= 4) // skip non-player element types (e.g. managers)
                .Select(FplTransformations.ToPlayer)
                .Where(p => clubIds.Contains(p.ClubId))
                .ToList();
            var fixtures = fplFixtures
                .Select(FplTransformations.ToFixture)
                .Where(f => clubIds.Contains(f.HomeClubId) && clubIds.Contains(f.AwayClubId))
                .ToList();

            _logger.LogInformation(
                "Extracted and transformed {Clubs} clubs, {Gameweeks} gameweeks, {Players} players, {Fixtures} fixtures",
                clubs.Count, gameweeks.Count, players.Count, fixtures.Count);

            // Load: order matters, players and fixtures reference clubs and gameweeks.
            await _fplDataRepository.BulkInsertOrUpdateClubsAsync(clubs);
            await _fplDataRepository.BulkInsertOrUpdateGameweeksAsync(gameweeks);
            await _fplDataRepository.BulkInsertOrUpdatePlayersAsync(players);
            await _fplDataRepository.BulkInsertOrUpdateFixturesAsync(fixtures);

            syncLog.ClubsLoaded = clubs.Count;
            syncLog.GameweeksLoaded = gameweeks.Count;
            syncLog.PlayersLoaded = players.Count;
            syncLog.FixturesLoaded = fixtures.Count;
            syncLog.Success = true;
            syncLog.CompletedAt = DateTime.UtcNow;
            syncLog.PredictionRecalculationQueued = await TryPublishSyncedMessageAsync(syncLog, cancellationToken);

            _logger.LogInformation("FPL ETL finished successfully at {Date}", syncLog.CompletedAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            syncLog.Success = false;
            syncLog.ErrorMessage = ex.Message;
            syncLog.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "An error occurred during the FPL ETL");
        }
        finally
        {
            await _etlSyncLogRepository.InsertAsync(syncLog);
        }

        return syncLog;
    }

    public async Task<List<EtlSyncLog>> GetLogsAsync(int count)
    {
        var result = await _etlSyncLogRepository.GetAllAsync(
            selector: x => x,
            orderBy: x => x.OrderByDescending(l => l.StartedAt),
            take: count);
        return result.ToList();
    }

    public async Task<EtlSyncLog> GetLogByIdAsync(Guid id)
    {
        return await _etlSyncLogRepository.GetAsync(selector: x => x, predicate: x => x.Id == id)
               ?? throw new NotFoundException(nameof(EtlSyncLog), id);
    }

    public async Task<EtlSyncLog> DeleteLogAsync(Guid id)
    {
        var log = await GetLogByIdAsync(id);
        return await _etlSyncLogRepository.DeleteAsync(log);
    }

    // The data is already stored, so a broker outage must not fail the ETL; predictions can
    // still be recalculated manually (POST /api/playerpredictions/recalculate).
    private async Task<bool> TryPublishSyncedMessageAsync(EtlSyncLog syncLog, CancellationToken cancellationToken)
    {
        try
        {
            var message = new FplDataSyncedMessage(Guid.NewGuid(), syncLog.CompletedAt!.Value, syncLog.PlayersLoaded,
                syncLog.FixturesLoaded);
            await _messagePublisher.PublishAsync(_rabbitMqSettings.FplDataSyncedQueue, message, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not queue prediction recalculation after the ETL.");
            return false;
        }
    }
}
