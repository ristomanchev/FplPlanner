using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EtlController : ControllerBase
{
    private readonly EtlMapper _etlMapper;

    public EtlController(EtlMapper etlMapper)
    {
        _etlMapper = etlMapper;
    }

    // Imports clubs, gameweeks, players and fixtures from the FPL API.
    [HttpPost("run")]
    public async Task<ActionResult<EtlResultResponse>> Run(CancellationToken cancellationToken)
    {
        return Ok(await _etlMapper.RunAsync(cancellationToken));
    }
}
