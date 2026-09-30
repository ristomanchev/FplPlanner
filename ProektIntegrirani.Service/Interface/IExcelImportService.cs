using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

public interface IExcelImportService
{
    // Reads and validates the rows only; saving is done by ISquadService.
    Task<ImportResult<SquadPickImportDto>> ImportSquadAsync(Stream fileStream);
}
