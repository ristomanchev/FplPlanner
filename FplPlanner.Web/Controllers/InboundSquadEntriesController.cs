using Microsoft.AspNetCore.Mvc;
using FplPlanner.Domain.Enums;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

// Internal view of the inbound log. Entries are created by external systems (POST /api/external/squads).
[Route("api/[controller]")]
[ApiController]
public class InboundSquadEntriesController : ControllerBase
{
    private readonly InboundSquadEntryMapper _inboundSquadEntryMapper;

    public InboundSquadEntriesController(InboundSquadEntryMapper inboundSquadEntryMapper)
    {
        _inboundSquadEntryMapper = inboundSquadEntryMapper;
    }

    // GET /api/inboundsquadentries?status=Failed
    [HttpGet]
    public async Task<ActionResult<List<InboundSquadEntryResponse>>> GetAll([FromQuery] InboundSquadStatus? status)
    {
        return Ok(await _inboundSquadEntryMapper.GetAllAsync(status));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InboundSquadEntryResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _inboundSquadEntryMapper.GetByIdAsync(id));
    }

    [HttpPut("{id:guid}/retry")]
    public async Task<ActionResult<InboundSquadEntryResponse>> Retry([FromRoute] Guid id)
    {
        return Ok(await _inboundSquadEntryMapper.RetryAsync(id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<InboundSquadEntryResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _inboundSquadEntryMapper.DeleteAsync(id));
    }
}
