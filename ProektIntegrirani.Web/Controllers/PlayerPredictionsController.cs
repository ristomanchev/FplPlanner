using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerPredictionsController : ControllerBase
{
    private readonly PlayerPredictionMapper _playerPredictionMapper;

    public PlayerPredictionsController(PlayerPredictionMapper playerPredictionMapper)
    {
        _playerPredictionMapper = playerPredictionMapper;
    }

    // GET /api/playerpredictions?gameweekNumber=6&position=Forward
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PlayerPredictionResponse>>> GetAll(
        [FromQuery] PlayerPredictionFilterRequest request)
    {
        return Ok(await _playerPredictionMapper.GetAllPagedAsync(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerPredictionResponse>> GetById([FromRoute] Guid id)
    {
        return Ok(await _playerPredictionMapper.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<PlayerPredictionResponse>> Insert([FromBody] PlayerPredictionRequest request)
    {
        var result = await _playerPredictionMapper.InsertAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlayerPredictionResponse>> Update([FromRoute] Guid id,
        [FromBody] PlayerPredictionRequest request)
    {
        return Ok(await _playerPredictionMapper.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<PlayerPredictionResponse>> Delete([FromRoute] Guid id)
    {
        return Ok(await _playerPredictionMapper.DeleteAsync(id));
    }
}
