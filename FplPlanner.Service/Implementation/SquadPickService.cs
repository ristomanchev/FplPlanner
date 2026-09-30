using Microsoft.EntityFrameworkCore;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

// CRUD for single picks. Rules that need the whole squad (15 players, 2/5/5/3, max 3 per club,
// formation) are checked by SquadService when a complete squad is saved.
public class SquadPickService : ISquadPickService
{
    private readonly IRepository<SquadPick> _repository;
    private readonly IRepository<Manager> _managerRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<Player> _playerRepository;

    public SquadPickService(IRepository<SquadPick> repository,
        IRepository<Manager> managerRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<Player> playerRepository)
    {
        _repository = repository;
        _managerRepository = managerRepository;
        _gameweekRepository = gameweekRepository;
        _playerRepository = playerRepository;
    }

    public async Task<List<SquadPick>> GetAllAsync(Guid? managerId, int? gameweekNumber)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: sp => (managerId == null || sp.ManagerId == managerId)
                             && (gameweekNumber == null || sp.Gameweek.Number == gameweekNumber),
            orderBy: x => x.OrderBy(sp => sp.Gameweek.Number).ThenBy(sp => sp.SquadPosition),
            include: x => x.Include(sp => sp.Manager)
                .Include(sp => sp.Gameweek)
                .Include(sp => sp.Player).ThenInclude(p => p.Club));
        return result.ToList();
    }

    public async Task<SquadPick> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id,
                   include: x => x.Include(sp => sp.Manager)
                       .Include(sp => sp.Gameweek)
                       .Include(sp => sp.Player).ThenInclude(p => p.Club))
               ?? throw new NotFoundException(nameof(SquadPick), id);
    }

    public async Task<SquadPick> InsertAsync(SquadPickDto dto)
    {
        await ValidateAsync(dto, excludedPickId: null);

        var pick = new SquadPick();
        Apply(pick, dto);
        await _repository.InsertAsync(pick);
        return await GetByIdAsync(pick.Id);
    }

    public async Task<SquadPick> UpdateAsync(Guid id, SquadPickDto dto)
    {
        var pick = await GetByIdAsync(id);
        await ValidateAsync(dto, excludedPickId: id);

        Apply(pick, dto);
        await _repository.UpdateAsync(pick);
        return await GetByIdAsync(id);
    }

    public async Task<SquadPick> DeleteAsync(Guid id)
    {
        var pick = await GetByIdAsync(id);
        return await _repository.DeleteAsync(pick);
    }

    private async Task ValidateAsync(SquadPickDto dto, Guid? excludedPickId)
    {
        if (!await _managerRepository.ExistsAsync(m => m.Id == dto.ManagerId))
        {
            throw new BusinessRuleException($"Manager with id {dto.ManagerId} does not exist.");
        }

        if (!await _gameweekRepository.ExistsAsync(g => g.Id == dto.GameweekId))
        {
            throw new BusinessRuleException($"Gameweek with id {dto.GameweekId} does not exist.");
        }

        if (!await _playerRepository.ExistsAsync(p => p.Id == dto.PlayerId))
        {
            throw new BusinessRuleException($"Player with id {dto.PlayerId} does not exist.");
        }

        if (dto.IsCaptain && dto.IsViceCaptain)
        {
            throw new BusinessRuleException("A player cannot be both captain and vice-captain.");
        }

        // Other picks in the same squad (same manager and gameweek).
        var squad = await _repository.GetAllAsync(
            selector: sp => new { sp.PlayerId, sp.SquadPosition, sp.IsCaptain, sp.IsViceCaptain },
            predicate: sp => sp.ManagerId == dto.ManagerId
                             && sp.GameweekId == dto.GameweekId
                             && sp.Id != excludedPickId);
        var others = squad.ToList();

        if (others.Any(sp => sp.PlayerId == dto.PlayerId))
        {
            throw new BusinessRuleException("This player is already in the squad for this gameweek.");
        }

        if (others.Any(sp => sp.SquadPosition == dto.SquadPosition))
        {
            throw new BusinessRuleException($"Squad position {dto.SquadPosition} is already taken.");
        }

        if (dto.IsCaptain && others.Any(sp => sp.IsCaptain))
        {
            throw new BusinessRuleException("The squad already has a captain.");
        }

        if (dto.IsViceCaptain && others.Any(sp => sp.IsViceCaptain))
        {
            throw new BusinessRuleException("The squad already has a vice-captain.");
        }
    }

    private static void Apply(SquadPick pick, SquadPickDto dto)
    {
        pick.ManagerId = dto.ManagerId;
        pick.GameweekId = dto.GameweekId;
        pick.PlayerId = dto.PlayerId;
        pick.SquadPosition = dto.SquadPosition;
        pick.IsCaptain = dto.IsCaptain;
        pick.IsViceCaptain = dto.IsViceCaptain;
    }
}
