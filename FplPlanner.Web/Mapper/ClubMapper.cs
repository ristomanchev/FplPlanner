using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class ClubMapper
{
    private readonly IClubService _clubService;

    public ClubMapper(IClubService clubService)
    {
        _clubService = clubService;
    }

    public async Task<List<ClubResponse>> GetAllAsync()
    {
        var result = await _clubService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<ClubResponse> GetByIdAsync(Guid id)
    {
        var result = await _clubService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<ClubResponse> InsertAsync(ClubRequest request)
    {
        var result = await _clubService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<ClubResponse> UpdateAsync(Guid id, ClubRequest request)
    {
        var result = await _clubService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<ClubResponse> DeleteAsync(Guid id)
    {
        var result = await _clubService.DeleteAsync(id);
        return result.ToResponse();
    }
}
