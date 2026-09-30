using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ManagersController : ControllerBase
{
    private readonly ManagerMapper _managerMapper;

    public ManagersController(ManagerMapper managerMapper)
    {
        _managerMapper = managerMapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManagerResponse>>> GetAll()
    {
        return Ok(await _managerMapper.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ManagerResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _managerMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ManagerResponse>> Insert([FromBody] ManagerRequest request)
    {
        var result = await _managerMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ManagerResponse>> Update([FromRoute] Guid id, [FromBody] ManagerRequest request)
    {
        return Ok(await _managerMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ManagerResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _managerMapper.DeleteAsync(id));
    }

    // Creates/updates the manager from the FPL API and imports their current squad.
    [HttpPost("import")]
    public async Task<ActionResult<ManagerResponse>> ImportFromFpl([FromBody] ManagerImportRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _managerMapper.ImportFromFplAsync(request, cancellationToken));
    }
}
