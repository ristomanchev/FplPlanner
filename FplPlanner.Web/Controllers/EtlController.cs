using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EtlController : ControllerBase
{
    private readonly EtlMapper _etlMapper;

    public EtlController(EtlMapper etlMapper)
    {
        _etlMapper = etlMapper;
    }

    // Imports clubs, gameweeks, players and fixtures from the FPL API; 502 if the FPL API or the load failed.
    [HttpPost("run")]
    public async Task<ActionResult<EtlSyncLogResponse>> Run(CancellationToken cancellationToken)
    {
        var result = await _etlMapper.RunAsync(cancellationToken);
        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status502BadGateway, result);
    }

    // GET /api/etl/logs?count=20
    [HttpGet("logs")]
    public async Task<ActionResult<List<EtlSyncLogResponse>>> GetLogs([FromQuery] int count = 20)
    {
        return Ok(await _etlMapper.GetLogsAsync(Math.Clamp(count, 1, 200)));
    }

    [HttpGet("logs/{id:guid}")]
    public async Task<ActionResult<EtlSyncLogResponse>> GetLog([FromRoute] Guid id)
    {
        return Ok(await _etlMapper.GetLogByIdAsync(id));
    }

    [HttpDelete("logs/{id:guid}")]
    public async Task<ActionResult<EtlSyncLogResponse>> DeleteLog([FromRoute] Guid id)
    {
        return Ok(await _etlMapper.DeleteLogAsync(id));
    }
}
