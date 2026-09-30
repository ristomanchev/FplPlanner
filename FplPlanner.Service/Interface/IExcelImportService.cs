using FplPlanner.Domain.Dto;

namespace FplPlanner.Service.Interface;

public interface IExcelImportService
{
    // Reads and validates the rows only; saving is done by ISquadService.
    Task<ImportResult<SquadPickImportDto>> ImportSquadAsync(Stream fileStream);
}
