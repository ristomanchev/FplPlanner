using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class FixtureMapper
{
    private readonly IFixtureService _fixtureService;

    public FixtureMapper(IFixtureService fixtureService)
    {
        _fixtureService = fixtureService;
    }

    public async Task<List<FixtureResponse>> GetAllAsync(int? gameweekNumber, Guid? clubId)
    {
        var result = await _fixtureService.GetAllAsync(gameweekNumber, clubId);
        return result.ToResponse();
    }

    public async Task<FixtureResponse> GetByIdAsync(Guid id)
    {
        var result = await _fixtureService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<FixtureResponse> InsertAsync(FixtureRequest request)
    {
        var result = await _fixtureService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<FixtureResponse> UpdateAsync(Guid id, FixtureRequest request)
    {
        var result = await _fixtureService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<FixtureResponse> DeleteAsync(Guid id)
    {
        var result = await _fixtureService.DeleteAsync(id);
        return result.ToResponse();
    }
}
