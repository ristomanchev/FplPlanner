using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Domain.Rules;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

public class ExcelService : IExcelService
{
    private const string SquadSheetName = "Squad";
    private static readonly string[] SquadHeaders = ["FplId", "Player", "Position", "Club", "SquadPosition", "Captain"];
    private const string CaptainMark = "C";
    private const string ViceCaptainMark = "V";

    private readonly IRepository<PlayerPrediction> _predictionRepository;
    private readonly IRepository<Gameweek> _gameweekRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly ISquadService _squadService;

    public ExcelService(IRepository<PlayerPrediction> predictionRepository,
        IRepository<Gameweek> gameweekRepository,
        IRepository<Player> playerRepository,
        ISquadService squadService)
    {
        _predictionRepository = predictionRepository;
        _gameweekRepository = gameweekRepository;
        _playerRepository = playerRepository;
        _squadService = squadService;
    }

    public async Task<ExcelFileDto> ExportPredictionsAsync(int horizon)
    {
        if (horizon is < 1 or > 38)
        {
            throw new BusinessRuleException("The horizon must be between 1 and 38 gameweeks.");
        }

        var gameweekNumbers = (await _gameweekRepository.GetAllAsync(
                selector: g => g.Number,
                predicate: g => !g.IsFinished,
                orderBy: x => x.OrderBy(g => g.Number)))
            .Take(horizon)
            .ToList();

        var predictions = (await _predictionRepository.GetAllAsync(
                selector: pp => pp,
                predicate: pp => gameweekNumbers.Contains(pp.Gameweek.Number)
                                 && pp.ModelType == PredictionModelType.Poisson,
                include: x => x.Include(pp => pp.Gameweek).Include(pp => pp.Player).ThenInclude(p => p.Club)))
            .ToList();

        if (predictions.Count == 0)
        {
            throw new BusinessRuleException("There are no predictions to export. Recalculate predictions first.");
        }

        using var workbook = new XLWorkbook();
        WriteExpectedPointsSheet(workbook, predictions, gameweekNumbers);
        WriteBreakdownSheet(workbook, predictions, gameweekNumbers[0]);

        return new ExcelFileDto
        {
            FileName = $"fpl-predictions-gw{gameweekNumbers[0]}-{gameweekNumbers[^1]}.xlsx",
            Content = Save(workbook)
        };
    }

    public async Task<ExcelFileDto> ExportSquadAsync(Guid managerId)
    {
        var squad = await _squadService.GetSquadAsync(managerId, gameweekNumber: null);
        var fplIds = (await _playerRepository.GetAllAsync(selector: p => new { p.Id, p.FplId }))
            .ToDictionary(p => p.Id, p => p.FplId);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(SquadSheetName);
        WriteHeader(sheet, SquadHeaders);

        var row = 2;
        foreach (var member in squad.Members.OrderBy(m => m.SquadPosition))
        {
            sheet.Cell(row, 1).Value = fplIds[member.PlayerId];
            sheet.Cell(row, 2).Value = member.WebName;
            sheet.Cell(row, 3).Value = member.Position.ToString();
            sheet.Cell(row, 4).Value = member.ClubShortName;
            sheet.Cell(row, 5).Value = member.SquadPosition;
            sheet.Cell(row, 6).Value = member.IsCaptain ? CaptainMark : member.IsViceCaptain ? ViceCaptainMark : "";
            row++;
        }

        sheet.Cell(row + 1, 1).Value =
            "Edit FplId / SquadPosition (1–11 start, 12–15 bench) / Captain (C or V), then import. Other columns are ignored.";
        sheet.Cell(row + 1, 1).Style.Font.Italic = true;
        sheet.Columns().AdjustToContents();

        return new ExcelFileDto
        {
            FileName = $"squad-{squad.TeamName.Replace(' ', '-').ToLowerInvariant()}-gw{squad.GameweekNumber}.xlsx",
            Content = Save(workbook)
        };
    }

    public async Task<SquadDto> ImportSquadAsync(Guid managerId, int gameweekNumber, Stream file)
    {
        var rows = ReadSquadRows(file);

        var fplIds = rows.Select(r => r.FplId).Distinct().ToList();
        var playerIdsByFplId = (await _playerRepository.GetAllAsync(
                selector: p => new { p.Id, p.FplId },
                predicate: p => fplIds.Contains(p.FplId)))
            .ToDictionary(p => p.FplId, p => p.Id);

        var unknown = rows.Where(r => !playerIdsByFplId.ContainsKey(r.FplId))
            .Select(r => $"Row {r.RowNumber}: no player with FPL id {r.FplId}.")
            .ToList();
        if (unknown.Count > 0)
        {
            throw new BusinessRuleException("The Excel file has invalid rows.", unknown);
        }

        var picks = rows.Select(r => new SquadPickInputDto
        {
            PlayerId = playerIdsByFplId[r.FplId],
            SquadPosition = r.SquadPosition,
            IsCaptain = r.IsCaptain,
            IsViceCaptain = r.IsViceCaptain
        }).ToList();

        // The same FPL rules as the API apply; an invalid squad is rejected with every violation listed.
        return await _squadService.SaveSquadAsync(managerId, gameweekNumber, picks);
    }

    private record SquadRow(int RowNumber, int FplId, int SquadPosition, bool IsCaptain, bool IsViceCaptain);

    private static List<SquadRow> ReadSquadRows(Stream file)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(file);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new BusinessRuleException("The file is not a valid .xlsx workbook.");
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == SquadSheetName) ?? workbook.Worksheet(1);
            var columns = sheet.Row(1).CellsUsed()
                .ToDictionary(c => c.GetString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

            var missing = new[] { "FplId", "SquadPosition", "Captain" }.Where(h => !columns.ContainsKey(h)).ToList();
            if (missing.Count > 0)
            {
                throw new BusinessRuleException($"Missing column(s): {string.Join(", ", missing)}.");
            }

            var rows = new List<SquadRow>();
            var errors = new List<string>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var fplIdCell = row.Cell(columns["FplId"]);
                if (fplIdCell.IsEmpty())
                {
                    continue;
                }

                if (!fplIdCell.TryGetValue(out int fplId))
                {
                    // Free text (e.g. the instructions line) is not a squad row.
                    if (!fplIdCell.Value.IsText) errors.Add($"Row {row.RowNumber()}: FplId must be a whole number.");
                    continue;
                }

                if (!row.Cell(columns["SquadPosition"]).TryGetValue(out int squadPosition)
                    || squadPosition is < 1 or > FplRules.SquadSize)
                {
                    errors.Add($"Row {row.RowNumber()}: SquadPosition must be a number from 1 to {FplRules.SquadSize}.");
                    continue;
                }

                var captain = row.Cell(columns["Captain"]).GetString().Trim().ToUpperInvariant();
                if (captain is not ("" or CaptainMark or ViceCaptainMark))
                {
                    errors.Add($"Row {row.RowNumber()}: Captain must be empty, '{CaptainMark}' or '{ViceCaptainMark}'.");
                    continue;
                }

                rows.Add(new SquadRow(row.RowNumber(), fplId, squadPosition, captain == CaptainMark,
                    captain == ViceCaptainMark));
            }

            if (errors.Count > 0)
            {
                throw new BusinessRuleException("The Excel file has invalid rows.", errors);
            }

            return rows;
        }
    }

    private static void WriteExpectedPointsSheet(XLWorkbook workbook, List<PlayerPrediction> predictions,
        List<int> gameweekNumbers)
    {
        var sheet = workbook.Worksheets.Add("Expected points");
        var headers = new[] { "Player", "Club", "Position", "Price", "Status" }
            .Concat(gameweekNumbers.Select(g => $"GW{g}"))
            .Append("Total")
            .ToArray();
        WriteHeader(sheet, headers);

        var byPlayer = predictions
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                g.First().Player,
                ByGameweek = g.ToDictionary(p => p.Gameweek.Number, p => p.ExpectedPoints),
                Total = g.Sum(p => p.ExpectedPoints)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var row = 2;
        foreach (var entry in byPlayer)
        {
            sheet.Cell(row, 1).Value = entry.Player.WebName;
            sheet.Cell(row, 2).Value = entry.Player.Club.ShortName;
            sheet.Cell(row, 3).Value = entry.Player.Position.ToString();
            sheet.Cell(row, 4).Value = entry.Player.Price;
            sheet.Cell(row, 5).Value = entry.Player.Status.ToString();

            for (var i = 0; i < gameweekNumbers.Count; i++)
            {
                sheet.Cell(row, 6 + i).Value = entry.ByGameweek.GetValueOrDefault(gameweekNumbers[i]);
            }

            sheet.Cell(row, 6 + gameweekNumbers.Count).Value = entry.Total;
            row++;
        }

        sheet.Column(4).Style.NumberFormat.Format = "0.0";
        sheet.Range(2, 6, row, headers.Length).Style.NumberFormat.Format = "0.00";
        FinishTable(sheet, headers.Length, row - 1);
    }

    private static void WriteBreakdownSheet(XLWorkbook workbook, List<PlayerPrediction> predictions,
        int gameweekNumber)
    {
        var sheet = workbook.Worksheets.Add($"GW{gameweekNumber} breakdown");
        string[] headers =
        [
            "Player", "Club", "Position", "Minutes", "Appearance", "Goals", "Assists", "Clean sheet",
            "Goals conceded", "Saves", "Def. contribution", "Bonus", "Cards", "Expected points"
        ];
        WriteHeader(sheet, headers);

        var row = 2;
        foreach (var p in predictions.Where(p => p.Gameweek.Number == gameweekNumber)
                     .OrderByDescending(p => p.ExpectedPoints))
        {
            var b = p.Breakdown;
            object[] values =
            [
                p.Player.WebName, p.Player.Club.ShortName, p.Player.Position.ToString(), p.ExpectedMinutes,
                b.Appearance, b.Goals, b.Assists, b.CleanSheet, b.GoalsConceded, b.Saves,
                b.DefensiveContribution, b.Bonus, b.Cards, p.ExpectedPoints
            ];
            for (var i = 0; i < values.Length; i++)
            {
                sheet.Cell(row, i + 1).Value = XLCellValue.FromObject(values[i]);
            }

            row++;
        }

        sheet.Range(2, 5, row, headers.Length).Style.NumberFormat.Format = "0.00";
        FinishTable(sheet, headers.Length, row - 1);
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }

        var header = sheet.Range(1, 1, 1, headers.Count);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#37003c");
    }

    private static void FinishTable(IXLWorksheet sheet, int columnCount, int lastRow)
    {
        sheet.Range(1, 1, Math.Max(lastRow, 1), columnCount).SetAutoFilter();
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
