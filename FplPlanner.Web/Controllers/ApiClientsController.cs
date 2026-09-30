using Microsoft.AspNetCore.Mvc;
using FplPlanner.Web.Mapper;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiClientsController : ControllerBase
{
    private readonly ApiClientMapper _apiClientMapper;

    public ApiClientsController(ApiClientMapper apiClientMapper)
    {
        _apiClientMapper = apiClientMapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiClientResponse>>> GetAll()
    {
        return Ok(await _apiClientMapper.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiClientResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _apiClientMapper.GetByIdAsync(id));
    }

    // The response contains the API key; it is not stored in plain text and cannot be shown again.
    [HttpPost]
    public async Task<ActionResult<ApiClientWithKeyResponse>> Insert([FromBody] ApiClientRequest request)
    {
        var result = await _apiClientMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiClientResponse>> Update([FromRoute] Guid id,
        [FromBody] ApiClientRequest request)
    {
        return Ok(await _apiClientMapper.UpdateAsync(id, request));
    }

    [HttpPost("{id:guid}/regenerate-key")]
    public async Task<ActionResult<ApiClientWithKeyResponse>> RegenerateKey([FromRoute] Guid id)
    {
        return Ok(await _apiClientMapper.RegenerateKeyAsync(id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiClientResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _apiClientMapper.DeleteAsync(id));
    }
}
