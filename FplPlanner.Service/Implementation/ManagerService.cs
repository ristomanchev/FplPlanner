using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.ExternalModels;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class ManagerService : IManagerService
{
    private readonly IRepository<Manager> _repository;

    public ManagerService(IRepository<Manager> repository)
    {
        _repository = repository;
    }

    public async Task<List<Manager>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            orderBy: x => x.OrderBy(m => m.TeamName));
        return result.ToList();
    }

    public async Task<Manager> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id)
               ?? throw new NotFoundException(nameof(Manager), id);
    }

    public async Task<Manager> InsertAsync(ManagerDto dto)
    {
        await EnsureEntryIdIsFreeAsync(dto.FplEntryId);

        var manager = new Manager { Id = GuidHelper.FromExternalId(nameof(Manager), dto.FplEntryId) };
        Apply(manager, dto);
        return await _repository.InsertAsync(manager);
    }

    public async Task<Manager> UpdateAsync(Guid id, ManagerDto dto)
    {
        var manager = await GetByIdAsync(id);
        // The external key determines the Id (GuidHelper), so it cannot change after creation.
        if (manager.FplEntryId != dto.FplEntryId)
        {
            throw new BusinessRuleException("The FPL entry id of an existing manager cannot be changed.");
        }

        Apply(manager, dto);
        return await _repository.UpdateAsync(manager);
    }

    // Squad picks are removed by the database (cascade delete).
    public async Task<Manager> DeleteAsync(Guid id)
    {
        var manager = await GetByIdAsync(id);
        return await _repository.DeleteAsync(manager);
    }

    private async Task EnsureEntryIdIsFreeAsync(int fplEntryId)
    {
        if (await _repository.ExistsAsync(m => m.FplEntryId == fplEntryId))
        {
            throw new BusinessRuleException($"A manager with FPL entry id {fplEntryId} already exists.");
        }
    }

    private static void Apply(Manager manager, ManagerDto dto)
    {
        manager.FplEntryId = dto.FplEntryId;
        manager.TeamName = dto.TeamName;
        manager.ManagerName = dto.ManagerName;
        manager.Email = dto.Email;
        manager.Bank = dto.Bank;
        manager.FreeTransfers = dto.FreeTransfers;
    }
}
