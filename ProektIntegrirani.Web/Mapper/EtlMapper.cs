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

    public async Task<EtlResultResponse> RunAsync(CancellationToken cancellationToken)
    {
        var result = await _fplEtlService.RunAsync(cancellationToken);
        return result.ToResponse();
    }
}
