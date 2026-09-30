using Microsoft.Extensions.Logging;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;
using FplPlanner.Service.Logic;

namespace FplPlanner.Service.Implementation;

public class PredictionService : IPredictionService
{
    private readonly IRepository<Club> _clubRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<Fixture> _fixtureRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<PlayerPrediction> _predictionRepository;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(IRepository<Club> clubRepository,
        IRepository<Player> playerRepository,
        IRepository<Fixture> fixtureRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<PlayerPrediction> predictionRepository,
        ILogger<PredictionService> logger)
    {
        _clubRepository = clubRepository;
        _playerRepository = playerRepository;
        _fixtureRepository = fixtureRepository;
        _gameweekRepository = gameweekRepository;
        _predictionRepository = predictionRepository;
        _logger = logger;
    }

    public async Task<PredictionRunResultDto> RecalculateAsync(int horizon = IPredictionService.DefaultHorizon,
        CancellationToken cancellationToken = default)
    {
        if (horizon is < 1 or > 38)
        {
            throw new BusinessRuleException("The horizon must be between 1 and 38 gameweeks.");
        }

        var gameweeks = (await _gameweekRepository.GetAllAsync(selector: g => g)).ToList();
        var upcoming = gameweeks.Where(g => !g.IsFinished).OrderBy(g => g.Number).Take(horizon).ToList();
        if (upcoming.Count == 0)
        {
            throw new BusinessRuleException("There are no upcoming gameweeks. Run the FPL ETL first.");
        }

        var clubs = (await _clubRepository.GetAllAsync(selector: c => c)).ToList();
        var players = (await _playerRepository.GetAllAsync(selector: p => p)).ToList();
        var fixtures = (await _fixtureRepository.GetAllAsync(selector: f => f)).ToList();

        var model = new PoissonPredictionModel(clubs, players, fixtures,
            gameweeks.ToDictionary(g => g.Id, g => g.Number),
            upcoming.Select(g => g.Number).ToList(),
            seasonStart: gameweeks.MinBy(g => g.Number)?.Deadline);

        var calculatedAt = DateTime.UtcNow;
        var gameweekIdsByNumber = upcoming.ToDictionary(g => g.Number, g => g.Id);
        var predictions = players
            .SelectMany(model.Predict)
            .Select(p => new PlayerPrediction
            {
                PlayerId = p.PlayerId,
                GameweekId = gameweekIdsByNumber[p.GameweekNumber],
                ModelType = PredictionModelType.Poisson,
                ExpectedMinutes = Math.Round((decimal)p.ExpectedMinutes, 1),
                Breakdown = p.Breakdown,
                ExpectedPoints = p.Breakdown.Total,
                CalculatedAt = calculatedAt
            })
            .ToList();

        // Replace, not merge: every run produces a complete set for the horizon.
        var upcomingIds = upcoming.Select(g => g.Id).ToList();
        var stale = await _predictionRepository.GetAllAsync(
            selector: pp => pp,
            predicate: pp => upcomingIds.Contains(pp.GameweekId) && pp.ModelType == PredictionModelType.Poisson);
        await _predictionRepository.DeleteManyAsync(stale.ToList());
        await _predictionRepository.InsertManyAsync(predictions);

        _logger.LogInformation("Saved {Count} predictions for gameweeks {From}–{To}",
            predictions.Count, upcoming.First().Number, upcoming.Last().Number);

        return new PredictionRunResultDto
        {
            GameweekNumbers = upcoming.Select(g => g.Number).ToList(),
            PlayersEvaluated = players.Count,
            PredictionsSaved = predictions.Count,
            CalculatedAt = calculatedAt
        };
    }
}
