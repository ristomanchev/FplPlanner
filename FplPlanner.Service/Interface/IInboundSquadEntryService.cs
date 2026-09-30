using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IInboundSquadEntryService
{
    Task<InboundSquadEntry> CreateAsync(string rawPayload, Guid apiClientId);
    Task<List<InboundSquadEntry>> GetAllAsync(InboundSquadStatus? status);
    Task<InboundSquadEntry> GetByIdAsync(Guid id);

    // Puts a Failed entry back to Pending so the processor tries it again.
    Task<InboundSquadEntry> RetryAsync(Guid id);
    Task<InboundSquadEntry> DeleteAsync(Guid id);
}
