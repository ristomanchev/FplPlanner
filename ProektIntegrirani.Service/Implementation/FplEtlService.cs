using Microsoft.Extensions.Logging;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.ExternalModels;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class FplEtlService : IFplEtlService
{
    private readonly IFplApiClient _fplApiClient;
    private readonly IRepository<Club> _clubRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<Fixture> _fixtureRepository;
    private readonly ILogger<FplEtlService> _logger;

    public FplEtlService(IFplApiClient fplApiClient,
        IRepository<Club> clubRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<Player> playerRepository,
        IRepository<Fixture> fixtureRepository,
        ILogger<FplEtlService> logger)
    {
        _fplApiClient = fplApiClient;
        _clubRepository = clubRepository;
        _gameweekRepository = gameweekRepository;
        _playerRepository = playerRepository;
        _fixtureRepository = fixtureRepository;
        _logger = logger;
    }

    public async Task<EtlResultDto> RunAsync(CancellationToken cancellationToken = default)
    {
        var result = new EtlResultDto { StartedAt = DateTime.UtcNow };

        // Extract
        var bootstrap = await _fplApiClient.GetBootstrapAsync(cancellationToken);
        var fixtures = await _fplApiClient.GetFixturesAsync(cancellationToken);

        // Transform + Load. Order matters: players and fixtures reference clubs and gameweeks.
        var clubIdsByFplId = await UpsertClubsAsync(bootstrap.Teams, result);
        var gameweekIdsByNumber = await UpsertGameweeksAsync(bootstrap.Events, result);
        await UpsertPlayersAsync(bootstrap.Elements, clubIdsByFplId, result);
        await UpsertFixturesAsync(fixtures, clubIdsByFplId, gameweekIdsByNumber, result);

        result.FinishedAt = DateTime.UtcNow;
        _logger.LogInformation(
            "FPL ETL finished: clubs +{ClubsInserted}/~{ClubsUpdated}, players +{PlayersInserted}/~{PlayersUpdated}, fixtures +{FixturesInserted}/~{FixturesUpdated}",
            result.ClubsInserted, result.ClubsUpdated, result.PlayersInserted, result.PlayersUpdated,
            result.FixturesInserted, result.FixturesUpdated);

        return result;
    }

    private async Task<Dictionary<int, Guid>> UpsertClubsAsync(List<FplTeam> teams, EtlResultDto result)
    {
        var existing = (await _clubRepository.GetAllAsync(selector: c => c)).ToDictionary(c => c.FplId);
        var inserted = new List<Club>();
        var updated = new List<Club>();

        foreach (var team in teams)
        {
            var isNew = !existing.TryGetValue(team.Id, out var club);
            club ??= new Club { FplId = team.Id };

            club.Name = team.Name;
            club.ShortName = team.ShortName;
            club.StrengthHome = FplTransformations.ToStrength(team.StrengthOverallHome);
            club.StrengthAway = FplTransformations.ToStrength(team.StrengthOverallAway);

            (isNew ? inserted : updated).Add(club);
        }

        await SaveAsync(_clubRepository, inserted, updated);
        result.ClubsInserted = inserted.Count;
        result.ClubsUpdated = updated.Count;

        return inserted.Concat(updated).ToDictionary(c => c.FplId, c => c.Id);
    }

    private async Task<Dictionary<int, Guid>> UpsertGameweeksAsync(List<FplEvent> events, EtlResultDto result)
    {
        var existing = (await _gameweekRepository.GetAllAsync(selector: g => g)).ToDictionary(g => g.Number);
        var inserted = new List<Gameweek>();
        var updated = new List<Gameweek>();

        foreach (var fplEvent in events)
        {
            var isNew = !existing.TryGetValue(fplEvent.Id, out var gameweek);
            gameweek ??= new Gameweek { Number = fplEvent.Id };

            gameweek.Deadline = fplEvent.DeadlineTime.ToUniversalTime();
            gameweek.IsFinished = fplEvent.Finished;

            (isNew ? inserted : updated).Add(gameweek);
        }

        await SaveAsync(_gameweekRepository, inserted, updated);
        result.GameweeksInserted = inserted.Count;
        result.GameweeksUpdated = updated.Count;

        return inserted.Concat(updated).ToDictionary(g => g.Number, g => g.Id);
    }

    private async Task UpsertPlayersAsync(List<FplElement> elements, Dictionary<int, Guid> clubIdsByFplId,
        EtlResultDto result)
    {
        var existing = (await _playerRepository.GetAllAsync(selector: p => p)).ToDictionary(p => p.FplId);
        var inserted = new List<Player>();
        var updated = new List<Player>();

        foreach (var element in elements)
        {
            if (!clubIdsByFplId.TryGetValue(element.Team, out var clubId) || element.ElementType is < 1 or > 4)
            {
                // e.g. the "manager" element type some seasons add; not a player.
                continue;
            }

            var isNew = !existing.TryGetValue(element.Id, out var player);
            player ??= new Player { FplId = element.Id };

            player.FirstName = element.FirstName;
            player.LastName = element.SecondName;
            player.WebName = element.WebName;
            player.Position = FplTransformations.ToPosition(element.ElementType);
            player.Price = FplTransformations.ToMillions(element.NowCost);
            player.Status = FplTransformations.ToPlayerStatus(element.Status);
            player.ChanceOfPlaying = element.ChanceOfPlayingNextRound;
            player.News = FplTransformations.ToNews(element.News);
            player.ClubId = clubId;
            player.Stats = FplTransformations.ToStats(element);

            (isNew ? inserted : updated).Add(player);
        }

        await SaveAsync(_playerRepository, inserted, updated);
        result.PlayersInserted = inserted.Count;
        result.PlayersUpdated = updated.Count;
    }

    private async Task UpsertFixturesAsync(List<FplFixture> fplFixtures, Dictionary<int, Guid> clubIdsByFplId,
        Dictionary<int, Guid> gameweekIdsByNumber, EtlResultDto result)
    {
        var existing = (await _fixtureRepository.GetAllAsync(selector: f => f)).ToDictionary(f => f.FplId);
        var inserted = new List<Fixture>();
        var updated = new List<Fixture>();

        foreach (var fplFixture in fplFixtures)
        {
            if (!clubIdsByFplId.TryGetValue(fplFixture.TeamH, out var homeClubId)
                || !clubIdsByFplId.TryGetValue(fplFixture.TeamA, out var awayClubId))
            {
                continue;
            }

            var isNew = !existing.TryGetValue(fplFixture.Id, out var fixture);
            fixture ??= new Fixture { FplId = fplFixture.Id };

            // A postponed fixture has no gameweek until FPL reschedules it.
            fixture.GameweekId = fplFixture.Event is { } number && gameweekIdsByNumber.TryGetValue(number, out var gwId)
                ? gwId
                : null;
            fixture.HomeClubId = homeClubId;
            fixture.AwayClubId = awayClubId;
            fixture.KickoffTime = fplFixture.KickoffTime?.ToUniversalTime();
            fixture.HomeScore = fplFixture.TeamHScore;
            fixture.AwayScore = fplFixture.TeamAScore;
            fixture.IsFinished = fplFixture.Finished;

            (isNew ? inserted : updated).Add(fixture);
        }

        await SaveAsync(_fixtureRepository, inserted, updated);
        result.FixturesInserted = inserted.Count;
        result.FixturesUpdated = updated.Count;
    }

    private static async Task SaveAsync<T>(IRepository<T> repository, List<T> inserted, List<T> updated)
        where T : Domain.Common.BaseEntity
    {
        if (inserted.Count > 0)
        {
            await repository.InsertManyAsync(inserted);
        }

        if (updated.Count > 0)
        {
            await repository.UpdateManyAsync(updated);
        }
    }
}
