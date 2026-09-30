using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SquadPicksController : ControllerBase
{
    private readonly SquadPickMapper _squadPickMapper;

    public SquadPicksController(SquadPickMapper squadPickMapper)
    {
        _squadPickMapper = squadPickMapper;
    }

    // GET /api/squadpicks?managerId=...&gameweekNumber=6
    [HttpGet]
    public async Task<ActionResult<List<SquadPickResponse>>> GetAll([FromQuery] Guid? managerId,
        [FromQuery] int? gameweekNumber)
    {
        return Ok(await _squadPickMapper.GetAllAsync(managerId, gameweekNumber));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SquadPickResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _squadPickMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<SquadPickResponse>> Insert([FromBody] SquadPickRequest request)
    {
        var result = await _squadPickMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SquadPickResponse>> Update([FromRoute] Guid id,
        [FromBody] SquadPickRequest request)
    {
        return Ok(await _squadPickMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<SquadPickResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _squadPickMapper.DeleteAsync(id));
    }
}
