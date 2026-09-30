using Microsoft.EntityFrameworkCore;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class InboundSquadEntryService : IInboundSquadEntryService
{
    private readonly IRepository<InboundSquadEntry> _repository;

    public InboundSquadEntryService(IRepository<InboundSquadEntry> repository)
    {
        _repository = repository;
    }

    public async Task<InboundSquadEntry> CreateAsync(string rawPayload, Guid apiClientId)
    {
        var entry = new InboundSquadEntry
        {
            RawPayload = rawPayload,
            ApiClientId = apiClientId,
            ReceivedAt = DateTime.UtcNow,
            Status = InboundSquadStatus.Pending
        };

        return await _repository.InsertAsync(entry);
    }

    public async Task<List<InboundSquadEntry>> GetAllAsync(InboundSquadStatus? status)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: e => status == null || e.Status == status,
            orderBy: x => x.OrderByDescending(e => e.ReceivedAt),
            include: x => x.Include(e => e.ApiClient));
        return result.ToList();
    }

    public async Task<InboundSquadEntry> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id,
                   include: x => x.Include(e => e.ApiClient))
               ?? throw new NotFoundException(nameof(InboundSquadEntry), id);
    }

    public async Task<InboundSquadEntry> RetryAsync(Guid id)
    {
        var entry = await GetByIdAsync(id);
        if (entry.Status != InboundSquadStatus.Failed)
        {
            throw new BusinessRuleException($"Only failed entries can be retried; this one is {entry.Status}.");
        }

        entry.Status = InboundSquadStatus.Pending;
        entry.ErrorMessage = null;
        entry.ProcessedAt = null;
        return await _repository.UpdateAsync(entry);
    }

    public async Task<InboundSquadEntry> DeleteAsync(Guid id)
    {
        var entry = await GetByIdAsync(id);
        if (entry.Status == InboundSquadStatus.Processing)
        {
            throw new BusinessRuleException("The entry is being processed right now.");
        }

        return await _repository.DeleteAsync(entry);
    }
}
