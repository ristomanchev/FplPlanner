using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

// Business functions on a manager's whole squad (validation, lineup, transfers).
[Route("api/[controller]")]
[ApiController]
public class SquadsController : ControllerBase
{
    private readonly SquadMapper _squadMapper;

    public SquadsController(SquadMapper squadMapper)
    {
        _squadMapper = squadMapper;
    }

    // GET /api/squads/{managerId}?gameweekNumber=5 (latest squad when omitted)
    [HttpGet("{managerId:guid}")]
    public async Task<ActionResult<SquadResponse>> GetSquad([FromRoute] Guid managerId,
        [FromQuery] int? gameweekNumber)
    {
        return Ok(await _squadMapper.GetSquadAsync(managerId, gameweekNumber));
    }

    // Replaces the whole squad; rejected with every broken FPL rule listed.
    [HttpPut("{managerId:guid}/gameweeks/{gameweekNumber:int}")]
    public async Task<ActionResult<SquadResponse>> SaveSquad([FromRoute] Guid managerId,
        [FromRoute] int gameweekNumber, [FromBody] SaveSquadRequest request)
    {
        return Ok(await _squadMapper.SaveSquadAsync(managerId, gameweekNumber, request));
    }

    [HttpGet("{managerId:guid}/validation")]
    public async Task<ActionResult<SquadValidationResponse>> Validate([FromRoute] Guid managerId,
        [FromQuery] int? gameweekNumber)
    {
        return Ok(await _squadMapper.ValidateAsync(managerId, gameweekNumber));
    }

    // Best starting XI, bench order and captain for the next gameweek.
    [HttpGet("{managerId:guid}/lineup")]
    public async Task<ActionResult<LineupResponse>> GetBestLineup([FromRoute] Guid managerId)
    {
        return Ok(await _squadMapper.GetBestLineupAsync(managerId));
    }

    // GET /api/squads/{managerId}/transfer-suggestions?horizon=5&maxTransfers=2
    [HttpGet("{managerId:guid}/transfer-suggestions")]
    public async Task<ActionResult<TransferAdviceResponse>> GetTransferSuggestions([FromRoute] Guid managerId,
        [FromQuery] TransferAdviceRequest request)
    {
        return Ok(await _squadMapper.GetTransferAdviceAsync(managerId, request));
    }
}
