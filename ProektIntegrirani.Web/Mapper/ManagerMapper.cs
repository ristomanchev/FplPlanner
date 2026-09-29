using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class ManagerMapper
{
    private readonly IManagerService _managerService;
    private readonly IFplManagerImportService _fplManagerImportService;

    public ManagerMapper(IManagerService managerService, IFplManagerImportService fplManagerImportService)
    {
        _managerService = managerService;
        _fplManagerImportService = fplManagerImportService;
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

    public async Task<ManagerResponse> ImportFromFplAsync(ManagerImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _fplManagerImportService.ImportAsync(request.FplEntryId, request.Email, cancellationToken);
        return result.ToResponse();
    }
}
