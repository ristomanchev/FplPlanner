namespace FplPlanner.Service.Interface;

public interface IExcelExportService
{
    // Expected points per player for the next `horizon` gameweeks, plus the breakdown for the next one.
    Task<byte[]> ExportPredictionsToExcel(int horizon);

    // The manager's latest squad in the import layout, so it can be edited and imported back.
    Task<byte[]> ExportSquadToExcel(Guid managerId);
}
