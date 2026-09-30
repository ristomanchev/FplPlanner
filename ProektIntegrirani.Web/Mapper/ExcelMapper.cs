using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class ExcelMapper
{
    private readonly IExcelService _excelService;

    public ExcelMapper(IExcelService excelService)
    {
        _excelService = excelService;
    }

    public Task<ExcelFileDto> ExportPredictionsAsync(int horizon)
    {
        return _excelService.ExportPredictionsAsync(horizon);
    }

    public Task<ExcelFileDto> ExportSquadAsync(Guid managerId)
    {
        return _excelService.ExportSquadAsync(managerId);
    }

    public async Task<SquadResponse> ImportSquadAsync(Guid managerId, int gameweekNumber, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var result = await _excelService.ImportSquadAsync(managerId, gameweekNumber, stream);
        return result.ToResponse();
    }
}
