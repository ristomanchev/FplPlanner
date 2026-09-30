using FplPlanner.Domain.Enums;
using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class InboundSquadEntryMapper
{
    private readonly IInboundSquadEntryService _inboundSquadEntryService;

    public InboundSquadEntryMapper(IInboundSquadEntryService inboundSquadEntryService)
    {
        _inboundSquadEntryService = inboundSquadEntryService;
    }

    public async Task<List<InboundSquadEntryResponse>> GetAllAsync(InboundSquadStatus? status)
    {
        var result = await _inboundSquadEntryService.GetAllAsync(status);
        return result.ToResponse();
    }

    public async Task<InboundSquadEntryResponse> GetByIdAsync(Guid id)
    {
        var result = await _inboundSquadEntryService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<InboundSquadEntryResponse> RetryAsync(Guid id)
    {
        await _inboundSquadEntryService.RetryAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<InboundSquadEntryResponse> DeleteAsync(Guid id)
    {
        var result = await _inboundSquadEntryService.DeleteAsync(id);
        return result.ToResponse();
    }
}
