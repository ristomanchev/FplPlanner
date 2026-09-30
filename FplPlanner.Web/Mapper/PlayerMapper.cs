using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class PlayerMapper
{
    private readonly IPlayerService _playerService;

    public PlayerMapper(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    public async Task<PaginatedResponse<PlayerResponse>> GetAllPagedAsync(PlayerFilterRequest request)
    {
        var result = await _playerService.GetAllPagedAsync(request.ToDto(), request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(p => p.ToResponse());
    }

    public async Task<PlayerResponse> GetByIdAsync(Guid id)
    {
        var result = await _playerService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<PlayerResponse> InsertAsync(PlayerRequest request)
    {
        var result = await _playerService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<PlayerResponse> UpdateAsync(Guid id, PlayerRequest request)
    {
        var result = await _playerService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<PlayerResponse> DeleteAsync(Guid id)
    {
        var result = await _playerService.DeleteAsync(id);
        return result.ToResponse();
    }
}
