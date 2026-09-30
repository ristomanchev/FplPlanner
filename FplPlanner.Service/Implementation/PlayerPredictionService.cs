using Microsoft.EntityFrameworkCore;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class PlayerPredictionService : IPlayerPredictionService
{
    private readonly IRepository<PlayerPrediction> _repository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;

    public PlayerPredictionService(IRepository<PlayerPrediction> repository,
        IRepository<Player> playerRepository,
        IRepository<Gameweek> gameweekRepository)
    {
        _repository = repository;
        _playerRepository = playerRepository;
        _gameweekRepository = gameweekRepository;
    }

    public async Task<PaginatedResult<PlayerPrediction>> GetAllPagedAsync(int? gameweekNumber, Position? position,
        int pageNumber, int pageSize)
    {
        return await _repository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: pp => (gameweekNumber == null || pp.Gameweek.Number == gameweekNumber)
                             && (position == null || pp.Player.Position == position),
            orderBy: x => x.OrderBy(pp => pp.Gameweek.Number).ThenByDescending(pp => pp.ExpectedPoints),
            include: x => x.Include(pp => pp.Gameweek).Include(pp => pp.Player).ThenInclude(p => p.Club),
            asNoTracking: true);
    }

    public async Task<List<PlayerPrediction>> GetForGameweeksAsync(List<int> gameweekNumbers)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: pp => gameweekNumbers.Contains(pp.Gameweek.Number)
                             && pp.ModelType == PredictionModelType.Poisson,
            orderBy: x => x.OrderByDescending(pp => pp.ExpectedPoints),
            include: x => x.Include(pp => pp.Gameweek).Include(pp => pp.Player).ThenInclude(p => p.Club));
        return result.ToList();
    }

    public async Task<PlayerPrediction> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id,
                   include: x => x.Include(pp => pp.Gameweek).Include(pp => pp.Player).ThenInclude(p => p.Club))
               ?? throw new NotFoundException(nameof(PlayerPrediction), id);
    }

    public async Task<PlayerPrediction> InsertAsync(PlayerPredictionDto dto)
    {
        await ValidateAsync(dto, excludedPredictionId: null);

        var prediction = new PlayerPrediction();
        Apply(prediction, dto);
        await _repository.InsertAsync(prediction);
        return await GetByIdAsync(prediction.Id);
    }

    public async Task<PlayerPrediction> UpdateAsync(Guid id, PlayerPredictionDto dto)
    {
        var prediction = await GetByIdAsync(id);
        await ValidateAsync(dto, excludedPredictionId: id);

        Apply(prediction, dto);
        await _repository.UpdateAsync(prediction);
        return await GetByIdAsync(id);
    }

    public async Task<PlayerPrediction> DeleteAsync(Guid id)
    {
        var prediction = await GetByIdAsync(id);
        return await _repository.DeleteAsync(prediction);
    }

    private async Task ValidateAsync(PlayerPredictionDto dto, Guid? excludedPredictionId)
    {
        if (!await _playerRepository.ExistsAsync(p => p.Id == dto.PlayerId))
        {
            throw new BusinessRuleException($"Player with id {dto.PlayerId} does not exist.");
        }

        if (!await _gameweekRepository.ExistsAsync(g => g.Id == dto.GameweekId))
        {
            throw new BusinessRuleException($"Gameweek with id {dto.GameweekId} does not exist.");
        }

        if (await _repository.ExistsAsync(pp => pp.PlayerId == dto.PlayerId
                                                && pp.GameweekId == dto.GameweekId
                                                && pp.ModelType == dto.ModelType
                                                && pp.Id != excludedPredictionId))
        {
            throw new BusinessRuleException("A prediction for this player, gameweek and model already exists.");
        }
    }

    private static void Apply(PlayerPrediction prediction, PlayerPredictionDto dto)
    {
        prediction.PlayerId = dto.PlayerId;
        prediction.GameweekId = dto.GameweekId;
        prediction.ModelType = dto.ModelType;
        prediction.ExpectedMinutes = dto.ExpectedMinutes;
        prediction.Breakdown = dto.Breakdown;
        prediction.ExpectedPoints = dto.Breakdown.Total;
        prediction.CalculatedAt = DateTime.UtcNow;
    }
}
