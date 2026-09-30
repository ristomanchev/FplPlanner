using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Service.Implementation;

public class SquadService : ISquadService
{
    private readonly IRepository<Manager> _managerRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<SquadPick> _squadPickRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<PlayerPrediction> _predictionRepository;

    public SquadService(IRepository<Manager> managerRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<SquadPick> squadPickRepository,
        IRepository<Player> playerRepository,
        IRepository<PlayerPrediction> predictionRepository)
    {
        _managerRepository = managerRepository;
        _gameweekRepository = gameweekRepository;
        _squadPickRepository = squadPickRepository;
        _playerRepository = playerRepository;
        _predictionRepository = predictionRepository;
    }

    public async Task<SquadDto> GetSquadAsync(Guid managerId, int? gameweekNumber)
    {
        var manager = await GetManagerAsync(managerId);
        var number = gameweekNumber ?? await GetLatestSquadGameweekAsync(managerId);
        var members = await LoadSquadAsync(managerId, number);

        if (members.Count == 0)
        {
            throw new NotFoundException($"{manager.TeamName} has no squad for gameweek {number}.");
        }

        // Predictions are optional here: the squad is shown even before the model has run.
        var nextGameweek = await GetNextGameweekNumberAsync();
        var expected = await LoadExpectedPointsAsync([nextGameweek], required: false);
        foreach (var member in members)
        {
            member.ExpectedPoints = Math.Round((decimal)expected.GetValueOrDefault(member.PlayerId), 2);
        }

        return new SquadDto
        {
            ManagerId = manager.Id,
            TeamName = manager.TeamName,
            GameweekNumber = number,
            Bank = manager.Bank,
            SquadValue = members.Sum(m => m.Price),
            Members = members,
            Validation = SquadValidator.Validate(members)
        };
    }

    public async Task<SquadDto> SaveSquadAsync(Guid managerId, int gameweekNumber, List<SquadPickInputDto> picks)
    {
        var manager = await GetManagerAsync(managerId);
        var gameweek = await _gameweekRepository.GetAsync(selector: g => g, predicate: g => g.Number == gameweekNumber)
                       ?? throw new NotFoundException($"Gameweek {gameweekNumber} was not found.");

        var playerIds = picks.Select(p => p.PlayerId).Distinct().ToList();
        var players = (await _playerRepository.GetAllAsync(
                selector: p => p,
                predicate: p => playerIds.Contains(p.Id),
                include: x => x.Include(p => p.Club)))
            .ToDictionary(p => p.Id);

        var unknown = playerIds.Where(id => !players.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
        {
            throw new BusinessRuleException($"Unknown player ids: {string.Join(", ", unknown)}.");
        }

        var members = picks.Select(pick => ToMember(players[pick.PlayerId], pick.SquadPosition, pick.IsCaptain,
            pick.IsViceCaptain)).ToList();
        var validation = SquadValidator.Validate(members);
        if (!validation.IsValid)
        {
            throw new SquadValidationException(validation.Errors);
        }

        var oldPicks = await _squadPickRepository.GetAllAsync(
            selector: sp => sp,
            predicate: sp => sp.ManagerId == manager.Id && sp.GameweekId == gameweek.Id);
        await _squadPickRepository.DeleteManyAsync(oldPicks.ToList());
        await _squadPickRepository.InsertManyAsync(picks.Select(pick => new SquadPick
        {
            ManagerId = manager.Id,
            GameweekId = gameweek.Id,
            PlayerId = pick.PlayerId,
            SquadPosition = pick.SquadPosition,
            IsCaptain = pick.IsCaptain,
            IsViceCaptain = pick.IsViceCaptain
        }).ToList());

        return await GetSquadAsync(managerId, gameweekNumber);
    }

    public async Task<SquadValidationResultDto> ValidateAsync(Guid managerId, int? gameweekNumber)
    {
        var squad = await GetSquadAsync(managerId, gameweekNumber);
        return squad.Validation;
    }

    public async Task<LineupDto> GetBestLineupAsync(Guid managerId)
    {
        await GetManagerAsync(managerId);
        var members = await LoadSquadAsync(managerId, await GetLatestSquadGameweekAsync(managerId));
        var nextGameweek = await GetNextGameweekNumberAsync();
        var expected = await LoadExpectedPointsAsync([nextGameweek], required: true);

        foreach (var member in members)
        {
            member.ExpectedPoints = Math.Round((decimal)expected.GetValueOrDefault(member.PlayerId), 2);
        }

        var lineup = SquadOptimizer.BestLineup(members, m => expected.GetValueOrDefault(m.PlayerId));
        return new LineupDto
        {
            GameweekNumber = nextGameweek,
            StartingEleven = lineup.StartingEleven,
            Bench = lineup.Bench,
            Captain = lineup.Captain,
            ViceCaptain = lineup.ViceCaptain,
            ExpectedPoints = Math.Round((decimal)lineup.StartingElevenScore, 2)
        };
    }

    public async Task<TransferAdviceDto> GetTransferAdviceAsync(Guid managerId, int horizon, int maxTransfers)
    {
        if (horizon is < 1 or > 8)
        {
            throw new BusinessRuleException("The horizon must be between 1 and 8 gameweeks.");
        }

        if (maxTransfers is < 1 or > 5)
        {
            throw new BusinessRuleException("Between 1 and 5 transfers can be planned.");
        }

        var manager = await GetManagerAsync(managerId);
        var squad = await LoadSquadAsync(managerId, await GetLatestSquadGameweekAsync(managerId));

        var gameweekNumbers = (await _gameweekRepository.GetAllAsync(
                selector: g => g.Number,
                predicate: g => !g.IsFinished,
                orderBy: x => x.OrderBy(g => g.Number)))
            .Take(horizon)
            .ToList();
        var scores = await LoadExpectedPointsAsync(gameweekNumbers, required: true);

        var allPlayers = (await _playerRepository.GetAllAsync(
                selector: p => new SquadMemberDto
                {
                    PlayerId = p.Id,
                    FplId = p.FplId,
                    WebName = p.WebName,
                    Position = p.Position,
                    ClubId = p.ClubId,
                    ClubShortName = p.Club.ShortName,
                    Price = p.Price,
                    Status = p.Status
                },
                predicate: p => p.Status != PlayerStatus.Unavailable))
            .ToList();

        // FPL's selling price (purchase price + half of any rise) is not public, so the current price is used.
        var advice = SquadOptimizer.SuggestTransfers(squad, allPlayers, scores, manager.Bank,
            manager.FreeTransfers, maxTransfers);
        advice.ManagerId = manager.Id;
        advice.GameweekNumbers = gameweekNumbers;
        return advice;
    }

    private async Task<Manager> GetManagerAsync(Guid managerId)
    {
        return await _managerRepository.GetAsync(selector: m => m, predicate: m => m.Id == managerId)
               ?? throw new NotFoundException(nameof(Manager), managerId);
    }

    private async Task<int> GetLatestSquadGameweekAsync(Guid managerId)
    {
        var latest = await _squadPickRepository.GetAsync(
            selector: sp => (int?)sp.Gameweek.Number,
            predicate: sp => sp.ManagerId == managerId,
            orderBy: x => x.OrderByDescending(sp => sp.Gameweek.Number));

        return latest ?? throw new NotFoundException("This manager has no squad yet.");
    }

    private async Task<int> GetNextGameweekNumberAsync()
    {
        var next = await _gameweekRepository.GetAsync(
            selector: g => (int?)g.Number,
            predicate: g => !g.IsFinished,
            orderBy: x => x.OrderBy(g => g.Number));

        return next ?? throw new BusinessRuleException("The season is over; there is no next gameweek.");
    }

    private async Task<List<SquadMemberDto>> LoadSquadAsync(Guid managerId, int gameweekNumber)
    {
        var picks = await _squadPickRepository.GetAllAsync(
            selector: sp => sp,
            predicate: sp => sp.ManagerId == managerId && sp.Gameweek.Number == gameweekNumber,
            orderBy: x => x.OrderBy(sp => sp.SquadPosition),
            include: x => x.Include(sp => sp.Player).ThenInclude(p => p.Club));

        return picks.Select(sp => ToMember(sp.Player, sp.SquadPosition, sp.IsCaptain, sp.IsViceCaptain)).ToList();
    }

    // Sum of expected points per player over the given gameweeks.
    private async Task<Dictionary<Guid, double>> LoadExpectedPointsAsync(List<int> gameweekNumbers, bool required)
    {
        var predictions = (await _predictionRepository.GetAllAsync(
                selector: pp => new { pp.PlayerId, pp.ExpectedPoints },
                predicate: pp => gameweekNumbers.Contains(pp.Gameweek.Number)
                                 && pp.ModelType == PredictionModelType.Poisson))
            .ToList();

        if (required && predictions.Count == 0)
        {
            throw new BusinessRuleException(
                $"No predictions for gameweek(s) {string.Join(", ", gameweekNumbers)}. Recalculate predictions first.");
        }

        return predictions
            .GroupBy(p => p.PlayerId)
            .ToDictionary(g => g.Key, g => g.Sum(p => (double)p.ExpectedPoints));
    }

    private static SquadMemberDto ToMember(Player player, int squadPosition, bool isCaptain, bool isViceCaptain)
    {
        return new SquadMemberDto
        {
            PlayerId = player.Id,
            FplId = player.FplId,
            WebName = player.WebName,
            Position = player.Position,
            ClubId = player.ClubId,
            ClubShortName = player.Club.ShortName,
            Price = player.Price,
            Status = player.Status,
            ChanceOfPlaying = player.ChanceOfPlaying,
            News = player.News,
            SquadPosition = squadPosition,
            IsCaptain = isCaptain,
            IsViceCaptain = isViceCaptain
        };
    }
}
