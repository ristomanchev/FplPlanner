using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClubsController : ControllerBase
{
    private readonly ClubMapper _clubMapper;

    public ClubsController(ClubMapper clubMapper)
    {
        _clubMapper = clubMapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClubResponse>>> GetAll()
    {
        return Ok(await _clubMapper.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _clubMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ClubResponse>> Insert([FromBody] ClubRequest request)
    {
        var result = await _clubMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClubResponse>> Update([FromRoute] Guid id, [FromBody] ClubRequest request)
    {
        return Ok(await _clubMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ClubResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _clubMapper.DeleteAsync(id));
    }
}
