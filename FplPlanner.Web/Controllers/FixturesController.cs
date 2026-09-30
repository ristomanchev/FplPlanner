using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FixturesController : ControllerBase
{
    private readonly FixtureMapper _fixtureMapper;

    public FixturesController(FixtureMapper fixtureMapper)
    {
        _fixtureMapper = fixtureMapper;
    }

    // GET /api/fixtures?gameweekNumber=6&clubId=...
    [HttpGet]
    public async Task<ActionResult<List<FixtureResponse>>> GetAll([FromQuery] int? gameweekNumber,
        [FromQuery] Guid? clubId)
    {
        return Ok(await _fixtureMapper.GetAllAsync(gameweekNumber, clubId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FixtureResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _fixtureMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<FixtureResponse>> Insert([FromBody] FixtureRequest request)
    {
        var result = await _fixtureMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FixtureResponse>> Update([FromRoute] Guid id, [FromBody] FixtureRequest request)
    {
        return Ok(await _fixtureMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<FixtureResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _fixtureMapper.DeleteAsync(id));
    }
}
