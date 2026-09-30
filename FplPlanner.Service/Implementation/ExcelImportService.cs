using ClosedXML.Excel;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Rules;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class ExcelImportService : IExcelImportService
{
    private const string CaptainMark = "C";
    private const string ViceCaptainMark = "V";

    private readonly IPlayerService _playerService;

    public ExcelImportService(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    public async Task<ImportResult<SquadPickImportDto>> ImportSquadAsync(Stream fileStream)
    {
        var result = new ImportResult<SquadPickImportDto>();

        using var workbook = OpenWorkbook(fileStream);
        var ws = workbook.Worksheet(1);

        var expectedHeaders = new Dictionary<string, int>();
        var headerRow = ws.Row(1);
        for (int col = 1; col <= (headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0); col++)
        {
            expectedHeaders[headerRow.Cell(col).GetString().Trim().ToLower()] = col;
        }

        var requiredHeaders = new[] { "fplid", "squadposition", "captain" };
        foreach (var h in requiredHeaders)
        {
            if (!expectedHeaders.ContainsKey(h))
            {
                result.Errors.Add(new ImportError
                {
                    Row = 1,
                    Column = h,
                    Message = $"Missing required column: '{h}'"
                });
            }
        }

        if (result.HasErrors) return result;

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        result.TotalRows = lastRow - 1;

        // One query for all players in the file instead of one per row.
        var fplIdCol = expectedHeaders["fplid"];
        var requestedFplIds = Enumerable.Range(2, Math.Max(0, lastRow - 1))
            .Select(r => ws.Cell(r, fplIdCol).TryGetValue(out int id) ? id : (int?)null)
            .OfType<int>()
            .Distinct()
            .ToList();

        var players = await _playerService.GetAllByFplIdsInAsync(requestedFplIds);
        var playerMap = players.ToDictionary(p => p.FplId);

        // Validate rows and build DTOs
        for (int row = 2; row <= lastRow; row++)
        {
            if (!ws.Cell(row, fplIdCol).TryGetValue(out int fplId))
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "FplId", Message = "FplId must be a whole number" });
                continue;
            }

            if (!playerMap.TryGetValue(fplId, out var player))
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "FplId", Message = $"Player with FPL id {fplId} not found in system" });
                continue;
            }

            if (!ws.Cell(row, expectedHeaders["squadposition"]).TryGetValue(out int squadPosition)
                || squadPosition is < 1 or > FplRules.SquadSize)
            {
                result.Errors.Add(new ImportError
                {
                    Row = row, Column = "SquadPosition",
                    Message = $"SquadPosition must be a number from 1 to {FplRules.SquadSize}"
                });
                continue;
            }

            var captain = ws.Cell(row, expectedHeaders["captain"]).GetString().Trim().ToUpperInvariant();
            if (captain is not ("" or CaptainMark or ViceCaptainMark))
            {
                result.Errors.Add(new ImportError
                {
                    Row = row, Column = "Captain",
                    Message = $"Captain must be empty, '{CaptainMark}' or '{ViceCaptainMark}'"
                });
                continue;
            }

            result.SuccessfulRecords.Add(new SquadPickImportDto
            {
                FplId = fplId,
                PlayerId = player.Id,
                WebName = player.WebName,
                SquadPosition = squadPosition,
                IsCaptain = captain == CaptainMark,
                IsViceCaptain = captain == ViceCaptainMark
            });
        }

        return result;
    }

    private static XLWorkbook OpenWorkbook(Stream fileStream)
    {
        try
        {
            return new XLWorkbook(fileStream);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new BusinessRuleException("The file is not a valid .xlsx workbook.");
        }
    }
}
