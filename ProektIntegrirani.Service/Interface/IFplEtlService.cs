using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

public interface IFplEtlService
{
    // Extract bootstrap-static and fixtures, transform them and upsert by FPL id.
    Task<EtlResultDto> RunAsync(CancellationToken cancellationToken = default);
}
