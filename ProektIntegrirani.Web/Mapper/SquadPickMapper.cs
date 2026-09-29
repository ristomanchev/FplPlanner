using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class SquadPickMapper
{
    private readonly ISquadPickService _squadPickService;

    public SquadPickMapper(ISquadPickService squadPickService)
    {
        _squadPickService = squadPickService;
    }

    public async Task<List<SquadPickResponse>> GetAllAsync(Guid? managerId, int? gameweekNumber)
    {
        var result = await _squadPickService.GetAllAsync(managerId, gameweekNumber);
        return result.ToResponse();
    }

    public async Task<SquadPickResponse> GetByIdAsync(Guid id)
    {
        var result = await _squadPickService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<SquadPickResponse> InsertAsync(SquadPickRequest request)
    {
        var result = await _squadPickService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<SquadPickResponse> UpdateAsync(Guid id, SquadPickRequest request)
    {
        var result = await _squadPickService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<SquadPickResponse> DeleteAsync(Guid id)
    {
        var result = await _squadPickService.DeleteAsync(id);
        return result.ToResponse();
    }
}
