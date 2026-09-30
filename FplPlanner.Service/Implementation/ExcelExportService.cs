using ClosedXML.Excel;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class ExcelExportService : IExcelExportService
{
    private readonly IPlayerPredictionService _playerPredictionService;
    private readonly IGameweekService _gameweekService;
    private readonly ISquadService _squadService;

    public ExcelExportService(IPlayerPredictionService playerPredictionService,
        IGameweekService gameweekService,
        ISquadService squadService)
    {
        _playerPredictionService = playerPredictionService;
        _gameweekService = gameweekService;
        _squadService = squadService;
    }

    public async Task<byte[]> ExportPredictionsToExcel(int horizon)
    {
        var gameweekNumbers = (await _gameweekService.GetUpcomingAsync(horizon)).Select(g => g.Number).ToList();
        var predictions = await _playerPredictionService.GetForGameweeksAsync(gameweekNumbers);

        if (predictions.Count == 0)
        {
            throw new BusinessRuleException("There are no predictions to export. Recalculate predictions first.");
        }

        using var workbook = new XLWorkbook();
        AddExpectedPointsSheet(workbook, predictions, gameweekNumbers);
        AddBreakdownSheet(workbook, predictions, gameweekNumbers[0]);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportSquadToExcel(Guid managerId)
    {
        var squad = await _squadService.GetSquadAsync(managerId, gameweekNumber: null);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Squad");

        var headers = new[] { "FplId", "Player", "Position", "Club", "SquadPosition", "Captain" };
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var member in squad.Members.OrderBy(m => m.SquadPosition))
        {
            ws.Cell(row, 1).Value = member.FplId;
            ws.Cell(row, 2).Value = member.WebName;
            ws.Cell(row, 3).Value = member.Position.ToString();
            ws.Cell(row, 4).Value = member.ClubShortName;
            ws.Cell(row, 5).Value = member.SquadPosition;
            ws.Cell(row, 6).Value = member.IsCaptain ? "C" : member.IsViceCaptain ? "V" : "";
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void AddExpectedPointsSheet(XLWorkbook workbook, List<PlayerPrediction> predictions,
        List<int> gameweekNumbers)
    {
        var ws = workbook.Worksheets.Add("Expected points");

        var headers = new[] { "Player", "Club", "Position", "Price", "Status" }
            .Concat(gameweekNumbers.Select(g => $"GW{g}"))
            .Append("Total")
            .ToArray();
        WriteHeader(ws, headers);

        var byPlayer = predictions
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                g.First().Player,
                ByGameweek = g.ToDictionary(p => p.Gameweek.Number, p => p.ExpectedPoints),
                Total = g.Sum(p => p.ExpectedPoints)
            })
            .OrderByDescending(x => x.Total);

        int row = 2;
        foreach (var entry in byPlayer)
        {
            ws.Cell(row, 1).Value = entry.Player.WebName;
            ws.Cell(row, 2).Value = entry.Player.Club.ShortName;
            ws.Cell(row, 3).Value = entry.Player.Position.ToString();
            ws.Cell(row, 4).Value = entry.Player.Price;
            ws.Cell(row, 5).Value = entry.Player.Status.ToString();
            for (int i = 0; i < gameweekNumbers.Count; i++)
            {
                ws.Cell(row, 6 + i).Value = entry.ByGameweek.GetValueOrDefault(gameweekNumbers[i]);
            }

            ws.Cell(row, headers.Length).Value = entry.Total;
            row++;
        }

        ws.Column(4).Style.NumberFormat.Format = "0.0";
        ws.Range(2, 6, Math.Max(row - 1, 2), headers.Length).Style.NumberFormat.Format = "0.00";
        FinishSheet(ws);
    }

    private static void AddBreakdownSheet(XLWorkbook workbook, List<PlayerPrediction> predictions,
        int gameweekNumber)
    {
        var ws = workbook.Worksheets.Add($"GW{gameweekNumber} breakdown");

        var headers = new[]
        {
            "Player", "Club", "Position", "Minutes", "Appearance", "Goals", "Assists", "Clean sheet",
            "Goals conceded", "Saves", "Def. contribution", "Bonus", "Cards", "Expected points"
        };
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var p in predictions.Where(p => p.Gameweek.Number == gameweekNumber)
                     .OrderByDescending(p => p.ExpectedPoints))
        {
            var b = p.Breakdown;
            ws.Cell(row, 1).Value = p.Player.WebName;
            ws.Cell(row, 2).Value = p.Player.Club.ShortName;
            ws.Cell(row, 3).Value = p.Player.Position.ToString();
            ws.Cell(row, 4).Value = p.ExpectedMinutes;
            ws.Cell(row, 5).Value = b.Appearance;
            ws.Cell(row, 6).Value = b.Goals;
            ws.Cell(row, 7).Value = b.Assists;
            ws.Cell(row, 8).Value = b.CleanSheet;
            ws.Cell(row, 9).Value = b.GoalsConceded;
            ws.Cell(row, 10).Value = b.Saves;
            ws.Cell(row, 11).Value = b.DefensiveContribution;
            ws.Cell(row, 12).Value = b.Bonus;
            ws.Cell(row, 13).Value = b.Cards;
            ws.Cell(row, 14).Value = p.ExpectedPoints;
            row++;
        }

        ws.Range(2, 5, Math.Max(row - 1, 2), headers.Length).Style.NumberFormat.Format = "0.00";
        FinishSheet(ws);
    }

    private static void WriteHeader(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        // Style the header
        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#37003C");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void FinishSheet(IXLWorksheet ws)
    {
        ws.Columns().AdjustToContents();
        ws.RangeUsed()?.SetAutoFilter();
        ws.SheetView.FreezeRows(1);
    }
}
