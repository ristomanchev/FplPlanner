using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.ExternalModels;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class ClubService : IClubService
{
    private readonly IRepository<Club> _repository;
    private readonly IRepository<Player> _playerRepository;
    private readonly IRepository<Fixture> _fixtureRepository;

    public ClubService(IRepository<Club> repository,
        IRepository<Player> playerRepository,
        IRepository<Fixture> fixtureRepository)
    {
        _repository = repository;
        _playerRepository = playerRepository;
        _fixtureRepository = fixtureRepository;
    }

    public async Task<List<Club>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            orderBy: x => x.OrderBy(c => c.Name));
        return result.ToList();
    }

    public async Task<Club> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id)
               ?? throw new NotFoundException(nameof(Club), id);
    }

    public async Task<Club> InsertAsync(ClubDto dto)
    {
        await EnsureFplIdIsFreeAsync(dto.FplId);

        var club = new Club { Id = GuidHelper.FromExternalId(nameof(Club), dto.FplId) };
        Apply(club, dto);
        return await _repository.InsertAsync(club);
    }

    public async Task<Club> UpdateAsync(Guid id, ClubDto dto)
    {
        var club = await GetByIdAsync(id);
        // The external key determines the Id (GuidHelper), so it cannot change after creation.
        if (club.FplId != dto.FplId)
        {
            throw new BusinessRuleException("The FPL id of an existing club cannot be changed.");
        }

        Apply(club, dto);
        return await _repository.UpdateAsync(club);
    }

    public async Task<Club> DeleteAsync(Guid id)
    {
        var club = await GetByIdAsync(id);

        if (await _playerRepository.ExistsAsync(p => p.ClubId == id))
        {
            throw new BusinessRuleException($"Club {club.Name} still has players and cannot be deleted.");
        }

        if (await _fixtureRepository.ExistsAsync(f => f.HomeClubId == id || f.AwayClubId == id))
        {
            throw new BusinessRuleException($"Club {club.Name} still has fixtures and cannot be deleted.");
        }

        return await _repository.DeleteAsync(club);
    }

    private async Task EnsureFplIdIsFreeAsync(int fplId)
    {
        if (await _repository.ExistsAsync(c => c.FplId == fplId))
        {
            throw new BusinessRuleException($"A club with FPL id {fplId} already exists.");
        }
    }

    private static void Apply(Club club, ClubDto dto)
    {
        club.FplId = dto.FplId;
        club.Name = dto.Name;
        club.ShortName = dto.ShortName;
        club.StrengthHome = dto.StrengthHome;
        club.StrengthAway = dto.StrengthAway;
    }
}
