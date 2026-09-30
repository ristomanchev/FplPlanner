using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameweeksController : ControllerBase
{
    private readonly GameweekMapper _gameweekMapper;

    public GameweeksController(GameweekMapper gameweekMapper)
    {
        _gameweekMapper = gameweekMapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<GameweekResponse>>> GetAll()
    {
        return Ok(await _gameweekMapper.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameweekResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _gameweekMapper.GetByIdAsync(id));
    }

    [HttpGet("next")]
    public async Task<ActionResult<GameweekResponse>> GetNext()
    {
        return Ok(await _gameweekMapper.GetNextAsync());
    }

    [HttpPost]
    public async Task<ActionResult<GameweekResponse>> Insert([FromBody] GameweekRequest request)
    {
        var result = await _gameweekMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GameweekResponse>> Update([FromRoute] Guid id, [FromBody] GameweekRequest request)
    {
        return Ok(await _gameweekMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GameweekResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _gameweekMapper.DeleteAsync(id));
    }
}
