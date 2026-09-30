using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExcelController : ControllerBase
{
    private const long MaxUploadBytes = 1024 * 1024;

    private readonly ExcelMapper _excelMapper;

    public ExcelController(ExcelMapper excelMapper)
    {
        _excelMapper = excelMapper;
    }

    // GET /api/excel/predictions?horizon=6
    [HttpGet("predictions")]
    public async Task<FileContentResult> ExportPredictions([FromQuery] int horizon = 6)
    {
        var file = await _excelMapper.ExportPredictionsAsync(horizon);
        return File(file.Content, ExcelFileDto.ContentType, file.FileName);
    }

    // The manager's latest squad; edit it and send it back to the import endpoint.
    [HttpGet("squads/{managerId:guid}")]
    public async Task<FileContentResult> ExportSquad([FromRoute] Guid managerId)
    {
        var file = await _excelMapper.ExportSquadAsync(managerId);
        return File(file.Content, ExcelFileDto.ContentType, file.FileName);
    }

    [HttpPost("squads/{managerId:guid}/gameweeks/{gameweekNumber:int}")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<SquadResponse>> ImportSquad([FromRoute] Guid managerId,
        [FromRoute] int gameweekNumber, IFormFile file)
    {
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(file), "Only .xlsx files are supported.");
            return ValidationProblem(ModelState);
        }

        return Ok(await _excelMapper.ImportSquadAsync(managerId, gameweekNumber, file));
    }
}
