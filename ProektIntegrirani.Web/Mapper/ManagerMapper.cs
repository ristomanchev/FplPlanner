using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class ManagerMapper
{
    private readonly IManagerService _managerService;

    public ManagerMapper(IManagerService managerService)
    {
        _managerService = managerService;
    }

    public async Task<List<ManagerResponse>> GetAllAsync()
    {
        var result = await _managerService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<ManagerResponse> GetByIdAsync(Guid id)
    {
        var result = await _managerService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<ManagerResponse> InsertAsync(ManagerRequest request)
    {
        var result = await _managerService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<ManagerResponse> UpdateAsync(Guid id, ManagerRequest request)
    {
        var result = await _managerService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<ManagerResponse> DeleteAsync(Guid id)
    {
        var result = await _managerService.DeleteAsync(id);
        return result.ToResponse();
    }
}
