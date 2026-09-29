using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class PlayerPredictionMapper
{
    private readonly IPlayerPredictionService _playerPredictionService;

    public PlayerPredictionMapper(IPlayerPredictionService playerPredictionService)
    {
        _playerPredictionService = playerPredictionService;
    }

    public async Task<PaginatedResponse<PlayerPredictionResponse>> GetAllPagedAsync(
        PlayerPredictionFilterRequest request)
    {
        var result = await _playerPredictionService.GetAllPagedAsync(request.GameweekNumber, request.Position,
            request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(p => p.ToResponse());
    }

    public async Task<PlayerPredictionResponse> GetByIdAsync(Guid id)
    {
        var result = await _playerPredictionService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<PlayerPredictionResponse> InsertAsync(PlayerPredictionRequest request)
    {
        var result = await _playerPredictionService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<PlayerPredictionResponse> UpdateAsync(Guid id, PlayerPredictionRequest request)
    {
        var result = await _playerPredictionService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<PlayerPredictionResponse> DeleteAsync(Guid id)
    {
        var result = await _playerPredictionService.DeleteAsync(id);
        return result.ToResponse();
    }
}
