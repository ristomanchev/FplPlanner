using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;

namespace ProektIntegrirani.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly IExcelImportService _importService;
    private readonly ISquadService _squadService;

    public ImportController(IExcelImportService importService, ISquadService squadService)
    {
        _importService = importService;
        _squadService = squadService;
    }

    // Replaces the manager's squad for the gameweek with the one in the sheet.
    [HttpPost("squads/{managerId:guid}/gameweeks/{gameweekNumber:int}")]
    public async Task<IActionResult> ImportSquad([FromRoute] Guid managerId, [FromRoute] int gameweekNumber,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
        {
            return BadRequest("Only .xlsx supported");
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest("File exceeds 5 MB");
        }

        await using var stream = file.OpenReadStream();
        var result = await _importService.ImportSquadAsync(stream);

        if (result.HasErrors)
        {
            return BadRequest(new
            {
                success = false,
                totalRows = result.TotalRows,
                successCount = result.SuccessfulRecords.Count,
                errorCount = result.Errors.Count,
                errors = result.Errors
            });
        }

        // Every row is readable; the squad still has to pass the FPL rules (400 with all violations if not).
        var squad = await _squadService.SaveSquadAsync(managerId, gameweekNumber, result.SuccessfulRecords.ToDto());

        return Ok(new
        {
            success = true,
            totalRows = result.TotalRows,
            squad = squad.ToResponse()
        });
    }

    [HttpGet("squads/get-import-template")]
    public IActionResult GetImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Squad Import");

        var headers = new[] { "FplId", "Player", "SquadPosition", "Captain" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        // Example rows: 15 rows in total; positions 1–11 start, 12–15 bench; Captain is C, V or empty.
        ws.Cell(2, 1).Value = 411;
        ws.Cell(2, 2).Value = "Haaland (optional, ignored)";
        ws.Cell(2, 3).Value = 10;
        ws.Cell(2, 4).Value = "C";

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), ExcelContentType, "squad-import-template.xlsx");
    }
}
