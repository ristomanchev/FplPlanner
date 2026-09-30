using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class GameweekMapper
{
    private readonly IGameweekService _gameweekService;

    public GameweekMapper(IGameweekService gameweekService)
    {
        _gameweekService = gameweekService;
    }

    public async Task<List<GameweekResponse>> GetAllAsync()
    {
        var result = await _gameweekService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<GameweekResponse> GetByIdAsync(Guid id)
    {
        var result = await _gameweekService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<GameweekResponse> GetNextAsync()
    {
        var result = await _gameweekService.GetNextAsync();
        return result.ToResponse();
    }

    public async Task<GameweekResponse> InsertAsync(GameweekRequest request)
    {
        var result = await _gameweekService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<GameweekResponse> UpdateAsync(Guid id, GameweekRequest request)
    {
        var result = await _gameweekService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<GameweekResponse> DeleteAsync(Guid id)
    {
        var result = await _gameweekService.DeleteAsync(id);
        return result.ToResponse();
    }
}
