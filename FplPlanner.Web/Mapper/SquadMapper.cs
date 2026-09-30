using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class SquadMapper
{
    private readonly ISquadService _squadService;

    public SquadMapper(ISquadService squadService)
    {
        _squadService = squadService;
    }

    public async Task<SquadResponse> GetSquadAsync(Guid managerId, int? gameweekNumber)
    {
        var result = await _squadService.GetSquadAsync(managerId, gameweekNumber);
        return result.ToResponse();
    }

    public async Task<SquadResponse> SaveSquadAsync(Guid managerId, int gameweekNumber, SaveSquadRequest request)
    {
        var result = await _squadService.SaveSquadAsync(managerId, gameweekNumber, request.ToDto());
        return result.ToResponse();
    }

    public async Task<SquadValidationResponse> ValidateAsync(Guid managerId, int? gameweekNumber)
    {
        var result = await _squadService.ValidateAsync(managerId, gameweekNumber);
        return result.ToResponse();
    }

    public async Task<LineupResponse> GetBestLineupAsync(Guid managerId)
    {
        var result = await _squadService.GetBestLineupAsync(managerId);
        return result.ToResponse();
    }

    public async Task<TransferAdviceResponse> GetTransferAdviceAsync(Guid managerId, TransferAdviceRequest request)
    {
        var result = await _squadService.GetTransferAdviceAsync(managerId, request.Horizon, request.MaxTransfers);
        return result.ToResponse();
    }
}
