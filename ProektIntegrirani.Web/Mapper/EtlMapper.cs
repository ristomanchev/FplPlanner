using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class EtlMapper
{
    private readonly IFplEtlService _fplEtlService;

    public EtlMapper(IFplEtlService fplEtlService)
    {
        _fplEtlService = fplEtlService;
    }

    public async Task<EtlSyncLogResponse> RunAsync(CancellationToken cancellationToken)
    {
        var result = await _fplEtlService.SyncAllAsync(cancellationToken);
        return result.ToResponse();
    }

    public async Task<List<EtlSyncLogResponse>> GetLogsAsync(int count)
    {
        var result = await _fplEtlService.GetLogsAsync(count);
        return result.ToResponse();
    }

    public async Task<EtlSyncLogResponse> GetLogByIdAsync(Guid id)
    {
        var result = await _fplEtlService.GetLogByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<EtlSyncLogResponse> DeleteLogAsync(Guid id)
    {
        var result = await _fplEtlService.DeleteLogAsync(id);
        return result.ToResponse();
    }
}
