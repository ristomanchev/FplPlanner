using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IExcelExportService _excelExportService;

    public ExportController(IExcelExportService excelExportService)
    {
        _excelExportService = excelExportService;
    }

    // GET /api/export/predictions?horizon=6
    [HttpGet("predictions")]
    public async Task<IActionResult> ExportPredictions([FromQuery] int horizon = 6)
    {
        var bytes = await _excelExportService.ExportPredictionsToExcel(horizon);
        return File(bytes, ExcelContentType, $"fpl-predictions-next-{horizon}.xlsx");
    }

    [HttpGet("squads/{managerId:guid}")]
    public async Task<IActionResult> ExportSquad([FromRoute] Guid managerId)
    {
        var bytes = await _excelExportService.ExportSquadToExcel(managerId);
        return File(bytes, ExcelContentType, $"squad-{managerId:N}.xlsx");
    }
}
