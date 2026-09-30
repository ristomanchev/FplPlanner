using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FplPlanner.Domain.Configuration;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.ExternalModels;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class FplManagerImportService : IFplManagerImportService
{
    private const int DefaultFreeTransfers = 1;

    private readonly IFplApiClient _fplApiClient;
    private readonly IRepository<Manager> _managerRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<SquadPick> _squadPickRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly FplApiSettings _settings;
    private readonly ILogger<FplManagerImportService> _logger;

    public FplManagerImportService(IFplApiClient fplApiClient,
        IRepository<Manager> managerRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<Player> playerRepository,
        IRepository<SquadPick> squadPickRepository,
        IMemoryCache memoryCache,
        IOptions<FplApiSettings> settings,
        ILogger<FplManagerImportService> logger)
    {
        _fplApiClient = fplApiClient;
        _managerRepository = managerRepository;
        _gameweekRepository = gameweekRepository;
        _playerRepository = playerRepository;
        _squadPickRepository = squadPickRepository;
        _memoryCache = memoryCache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Manager> ImportAsync(int fplEntryId, string? email,
        CancellationToken cancellationToken = default)
    {
        var entry = await GetCachedAsync($"fpl-api:entry:{fplEntryId}",
            () => _fplApiClient.GetEntryAsync(fplEntryId, cancellationToken));
        var manager = await UpsertManagerAsync(entry.Id, entry.Name,
            $"{entry.PlayerFirstName} {entry.PlayerLastName}".Trim(),
            FplTransformations.ToMillions(entry.LastDeadlineBank ?? 0), email);

        if (entry.CurrentEvent is { } currentEvent)
        {
            await ImportPicksAsync(manager, currentEvent, cancellationToken);
        }

        return manager;
    }

    private async Task<Manager> UpsertManagerAsync(int entryId, string teamName, string managerName, decimal bank,
        string? email)
    {
        var manager = await _managerRepository.GetAsync(selector: m => m, predicate: m => m.FplEntryId == entryId);
        if (manager == null)
        {
            manager = new Manager
            {
                Id = GuidHelper.FromExternalId(nameof(Manager), entryId),
                FplEntryId = entryId,
                TeamName = teamName,
                ManagerName = managerName,
                Bank = bank,
                Email = email,
                FreeTransfers = DefaultFreeTransfers
            };
            return await _managerRepository.InsertAsync(manager);
        }

        manager.TeamName = teamName;
        manager.ManagerName = managerName;
        manager.Bank = bank;
        manager.Email = email ?? manager.Email;
        return await _managerRepository.UpdateAsync(manager);
    }

    private async Task ImportPicksAsync(Manager manager, int gameweekNumber, CancellationToken cancellationToken)
    {
        var gameweek = await _gameweekRepository.GetAsync(selector: g => g, predicate: g => g.Number == gameweekNumber)
                       ?? throw new BusinessRuleException(
                           $"Gameweek {gameweekNumber} is not in the database. Run the FPL ETL first.");

        var fplPicks = await GetCachedAsync($"fpl-api:picks:{manager.FplEntryId}:{gameweekNumber}",
            () => _fplApiClient.GetPicksAsync(manager.FplEntryId, gameweekNumber, cancellationToken));
        var elementIds = fplPicks.Picks.Select(p => p.Element).ToList();

        var playerIdsByFplId = (await _playerRepository.GetAllAsync(
                selector: p => new { p.Id, p.FplId },
                predicate: p => elementIds.Contains(p.FplId)))
            .ToDictionary(p => p.FplId, p => p.Id);

        var missing = elementIds.Where(id => !playerIdsByFplId.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new BusinessRuleException(
                $"Players with FPL ids {string.Join(", ", missing)} are not in the database. Run the FPL ETL first.");
        }

        // Replace the stored squad for that gameweek with the one from FPL.
        var oldPicks = await _squadPickRepository.GetAllAsync(
            selector: sp => sp,
            predicate: sp => sp.ManagerId == manager.Id && sp.GameweekId == gameweek.Id);
        await _squadPickRepository.DeleteManyAsync(oldPicks.ToList());

        var newPicks = fplPicks.Picks.Select(p => new SquadPick
        {
            ManagerId = manager.Id,
            GameweekId = gameweek.Id,
            PlayerId = playerIdsByFplId[p.Element],
            SquadPosition = p.Position,
            IsCaptain = p.IsCaptain,
            IsViceCaptain = p.IsViceCaptain
        }).ToList();
        await _squadPickRepository.InsertManyAsync(newPicks);
    }

    // Re-importing the same manager within a few minutes reuses the FPL response instead of calling the API again.
    private async Task<T> GetCachedAsync<T>(string cacheKey, Func<Task<T>> fetch) where T : class
    {
        if (_memoryCache.TryGetValue(cacheKey, out T? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var apiData = await fetch();
        _memoryCache.Set(cacheKey, apiData, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes));
        return apiData;
    }
}
