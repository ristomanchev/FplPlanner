using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class FixtureService : IFixtureService
{
    private readonly IRepository<Fixture> _repository;
    private readonly IRepository<Club> _clubRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;

    public FixtureService(IRepository<Fixture> repository,
        IRepository<Club> clubRepository,
        IRepository<Gameweek> gameweekRepository)
    {
        _repository = repository;
        _clubRepository = clubRepository;
        _gameweekRepository = gameweekRepository;
    }

    public async Task<List<Fixture>> GetAllAsync(int? gameweekNumber, Guid? clubId)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: f => (gameweekNumber == null || f.Gameweek != null && f.Gameweek.Number == gameweekNumber)
                            && (clubId == null || f.HomeClubId == clubId || f.AwayClubId == clubId),
            orderBy: x => x.OrderBy(f => f.KickoffTime).ThenBy(f => f.FplId),
            include: x => x.Include(f => f.HomeClub).Include(f => f.AwayClub).Include(f => f.Gameweek));
        return result.ToList();
    }

    public async Task<Fixture> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id,
                   include: x => x.Include(f => f.HomeClub).Include(f => f.AwayClub).Include(f => f.Gameweek))
               ?? throw new NotFoundException(nameof(Fixture), id);
    }

    public async Task<Fixture> InsertAsync(FixtureDto dto)
    {
        await EnsureFplIdIsFreeAsync(dto.FplId);
        await ValidateRelationsAsync(dto);

        var fixture = new Fixture();
        Apply(fixture, dto);
        await _repository.InsertAsync(fixture);
        return await GetByIdAsync(fixture.Id);
    }

    public async Task<Fixture> UpdateAsync(Guid id, FixtureDto dto)
    {
        var fixture = await GetByIdAsync(id);
        if (fixture.FplId != dto.FplId)
        {
            await EnsureFplIdIsFreeAsync(dto.FplId);
        }

        await ValidateRelationsAsync(dto);

        Apply(fixture, dto);
        await _repository.UpdateAsync(fixture);
        return await GetByIdAsync(id);
    }

    public async Task<Fixture> DeleteAsync(Guid id)
    {
        var fixture = await GetByIdAsync(id);
        return await _repository.DeleteAsync(fixture);
    }

    private async Task EnsureFplIdIsFreeAsync(int fplId)
    {
        if (await _repository.ExistsAsync(f => f.FplId == fplId))
        {
            throw new BusinessRuleException($"A fixture with FPL id {fplId} already exists.");
        }
    }

    private async Task ValidateRelationsAsync(FixtureDto dto)
    {
        if (dto.HomeClubId == dto.AwayClubId)
        {
            throw new BusinessRuleException("A club cannot play against itself.");
        }

        if (!await _clubRepository.ExistsAsync(c => c.Id == dto.HomeClubId))
        {
            throw new BusinessRuleException($"Home club with id {dto.HomeClubId} does not exist.");
        }

        if (!await _clubRepository.ExistsAsync(c => c.Id == dto.AwayClubId))
        {
            throw new BusinessRuleException($"Away club with id {dto.AwayClubId} does not exist.");
        }

        if (dto.GameweekId != null && !await _gameweekRepository.ExistsAsync(g => g.Id == dto.GameweekId))
        {
            throw new BusinessRuleException($"Gameweek with id {dto.GameweekId} does not exist.");
        }

        if (dto.IsFinished && (dto.HomeScore == null || dto.AwayScore == null))
        {
            throw new BusinessRuleException("A finished fixture must have both scores.");
        }
    }

    private static void Apply(Fixture fixture, FixtureDto dto)
    {
        fixture.FplId = dto.FplId;
        fixture.GameweekId = dto.GameweekId;
        fixture.HomeClubId = dto.HomeClubId;
        fixture.AwayClubId = dto.AwayClubId;
        fixture.KickoffTime = dto.KickoffTime;
        fixture.HomeScore = dto.HomeScore;
        fixture.AwayScore = dto.AwayScore;
        fixture.IsFinished = dto.IsFinished;
    }
}
