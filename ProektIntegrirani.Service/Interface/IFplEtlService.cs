using ProektIntegrirani.Domain.Models;

namespace ProektIntegrirani.Service.Interface;

public interface IFplEtlService
{
    // Extract bootstrap-static and fixtures, transform them and bulk upsert; the run is recorded in EtlSyncLog.
    Task<EtlSyncLog> SyncAllAsync(CancellationToken cancellationToken = default);

    Task<List<EtlSyncLog>> GetLogsAsync(int count);
    Task<EtlSyncLog> GetLogByIdAsync(Guid id);
    Task<EtlSyncLog> DeleteLogAsync(Guid id);
}
