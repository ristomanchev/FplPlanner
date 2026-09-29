using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly PlayerMapper _playerMapper;

    public PlayersController(PlayerMapper playerMapper)
    {
        _playerMapper = playerMapper;
    }

    // GET /api/players?position=Midfielder&maxPrice=8&search=sal&pageNumber=0&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PlayerResponse>>> GetAll([FromQuery] PlayerFilterRequest request)
    {
        return Ok(await _playerMapper.GetAllPagedAsync(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _playerMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<PlayerResponse>> Insert([FromBody] PlayerRequest request)
    {
        var result = await _playerMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlayerResponse>> Update([FromRoute] Guid id, [FromBody] PlayerRequest request)
    {
        return Ok(await _playerMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<PlayerResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _playerMapper.DeleteAsync(id));
    }
}
