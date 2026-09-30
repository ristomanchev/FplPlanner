using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

public interface IExcelService
{
    // Expected points per player for the next `horizon` gameweeks, plus the breakdown for the next one.
    Task<ExcelFileDto> ExportPredictionsAsync(int horizon);

    // The manager's latest squad in the same layout the import expects (a ready-made template).
    Task<ExcelFileDto> ExportSquadAsync(Guid managerId);

    // Reads a squad sheet (FplId, SquadPosition, Captain) and saves it through the squad rules.
    Task<SquadDto> ImportSquadAsync(Guid managerId, int gameweekNumber, Stream file);
}
