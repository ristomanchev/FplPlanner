using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.ExternalModels;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class PlayerService : IPlayerService
{
    private readonly IRepository<Player> _repository;
    private readonly IRepository<Club> _clubRepository;
    private readonly IRepository<SquadPick> _squadPickRepository;

    public PlayerService(IRepository<Player> repository,
        IRepository<Club> clubRepository,
        IRepository<SquadPick> squadPickRepository)
    {
        _repository = repository;
        _clubRepository = clubRepository;
        _squadPickRepository = squadPickRepository;
    }

    public async Task<PaginatedResult<Player>> GetAllPagedAsync(PlayerFilterDto filter, int pageNumber, int pageSize)
    {
        var search = filter.Search?.Trim().ToLower();

        return await _repository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: p => (filter.Position == null || p.Position == filter.Position)
                            && (filter.ClubId == null || p.ClubId == filter.ClubId)
                            && (filter.MaxPrice == null || p.Price <= filter.MaxPrice)
                            && (search == null || p.WebName.ToLower().Contains(search)
                                               || p.LastName.ToLower().Contains(search)),
            orderBy: x => x.OrderByDescending(p => p.Stats.TotalPoints).ThenBy(p => p.WebName),
            include: x => x.Include(p => p.Club),
            asNoTracking: true);
    }

    public async Task<Player> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id,
                   include: x => x.Include(p => p.Club))
               ?? throw new NotFoundException(nameof(Player), id);
    }

    public async Task<List<Player>> GetAllByFplIdsInAsync(List<int> fplIds)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: p => fplIds.Contains(p.FplId),
            include: x => x.Include(p => p.Club));
        return result.ToList();
    }

    public async Task<Player> InsertAsync(PlayerDto dto)
    {
        await EnsureFplIdIsFreeAsync(dto.FplId);
        await EnsureClubExistsAsync(dto.ClubId);

        var player = new Player { Id = GuidHelper.FromExternalId(nameof(Player), dto.FplId) };
        Apply(player, dto);
        await _repository.InsertAsync(player);

        // Reload so the Club navigation is populated for the response.
        return await GetByIdAsync(player.Id);
    }

    public async Task<Player> UpdateAsync(Guid id, PlayerDto dto)
    {
        var player = await GetByIdAsync(id);
        // The external key determines the Id (GuidHelper), so it cannot change after creation.
        if (player.FplId != dto.FplId)
        {
            throw new BusinessRuleException("The FPL id of an existing player cannot be changed.");
        }

        await EnsureClubExistsAsync(dto.ClubId);

        Apply(player, dto);
        await _repository.UpdateAsync(player);
        return await GetByIdAsync(id);
    }

    public async Task<Player> DeleteAsync(Guid id)
    {
        var player = await GetByIdAsync(id);

        if (await _squadPickRepository.ExistsAsync(sp => sp.PlayerId == id))
        {
            throw new BusinessRuleException($"{player.WebName} is picked in a squad and cannot be deleted.");
        }

        return await _repository.DeleteAsync(player);
    }

    private async Task EnsureFplIdIsFreeAsync(int fplId)
    {
        if (await _repository.ExistsAsync(p => p.FplId == fplId))
        {
            throw new BusinessRuleException($"A player with FPL id {fplId} already exists.");
        }
    }

    private async Task EnsureClubExistsAsync(Guid clubId)
    {
        if (!await _clubRepository.ExistsAsync(c => c.Id == clubId))
        {
            throw new BusinessRuleException($"Club with id {clubId} does not exist.");
        }
    }

    // Stats are owned by the ETL and intentionally not touched here.
    private static void Apply(Player player, PlayerDto dto)
    {
        player.FplId = dto.FplId;
        player.FirstName = dto.FirstName;
        player.LastName = dto.LastName;
        player.WebName = dto.WebName;
        player.Position = dto.Position;
        player.Price = dto.Price;
        player.Status = dto.Status;
        player.ChanceOfPlaying = dto.ChanceOfPlaying;
        player.News = dto.News;
        player.ClubId = dto.ClubId;
    }
}
