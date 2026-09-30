using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.ExternalModels;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class GameweekService : IGameweekService
{
    private readonly IRepository<Gameweek> _repository;
    private readonly IRepository<SquadPick> _squadPickRepository;

    public GameweekService(IRepository<Gameweek> repository, IRepository<SquadPick> squadPickRepository)
    {
        _repository = repository;
        _squadPickRepository = squadPickRepository;
    }

    public async Task<List<Gameweek>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            orderBy: x => x.OrderBy(g => g.Number));
        return result.ToList();
    }

    public async Task<Gameweek> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id)
               ?? throw new NotFoundException(nameof(Gameweek), id);
    }

    public async Task<Gameweek> GetByNumberAsync(int number)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Number == number)
               ?? throw new NotFoundException($"Gameweek {number} was not found.");
    }

    public async Task<Gameweek> GetNextAsync()
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => !x.IsFinished,
                   orderBy: x => x.OrderBy(g => g.Number))
               ?? throw new NotFoundException("There is no upcoming gameweek.");
    }

    public async Task<List<Gameweek>> GetUpcomingAsync(int count)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: x => !x.IsFinished,
            orderBy: x => x.OrderBy(g => g.Number));
        return result.Take(count).ToList();
    }

    public async Task<Gameweek> InsertAsync(GameweekDto dto)
    {
        await EnsureNumberIsFreeAsync(dto.Number);

        var gameweek = new Gameweek { Id = GuidHelper.FromExternalId(nameof(Gameweek), dto.Number) };
        Apply(gameweek, dto);
        return await _repository.InsertAsync(gameweek);
    }

    public async Task<Gameweek> UpdateAsync(Guid id, GameweekDto dto)
    {
        var gameweek = await GetByIdAsync(id);
        // The external key determines the Id (GuidHelper), so it cannot change after creation.
        if (gameweek.Number != dto.Number)
        {
            throw new BusinessRuleException("The number of an existing gameweek cannot be changed.");
        }

        Apply(gameweek, dto);
        return await _repository.UpdateAsync(gameweek);
    }

    public async Task<Gameweek> DeleteAsync(Guid id)
    {
        var gameweek = await GetByIdAsync(id);

        if (await _squadPickRepository.ExistsAsync(sp => sp.GameweekId == id))
        {
            throw new BusinessRuleException($"Gameweek {gameweek.Number} has squad picks and cannot be deleted.");
        }

        return await _repository.DeleteAsync(gameweek);
    }

    private async Task EnsureNumberIsFreeAsync(int number)
    {
        if (await _repository.ExistsAsync(g => g.Number == number))
        {
            throw new BusinessRuleException($"Gameweek {number} already exists.");
        }
    }

    private static void Apply(Gameweek gameweek, GameweekDto dto)
    {
        gameweek.Number = dto.Number;
        gameweek.Deadline = dto.Deadline;
        gameweek.IsFinished = dto.IsFinished;
    }
}
