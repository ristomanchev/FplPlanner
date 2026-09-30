using FplPlanner.Service.Interface;
using FplPlanner.Web.Extensions;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Mapper;

public class ReportMapper
{
    private readonly IWeeklyReportService _weeklyReportService;

    public ReportMapper(IWeeklyReportService weeklyReportService)
    {
        _weeklyReportService = weeklyReportService;
    }

    public async Task<WeeklyReportResponse> GetWeeklyReportAsync(Guid managerId)
    {
        var result = await _weeklyReportService.BuildAsync(managerId);
        return result.ToResponse();
    }

    public Task<string> GetWeeklyReportHtmlAsync(Guid managerId)
    {
        return _weeklyReportService.RenderHtmlAsync(managerId);
    }

    public async Task<WeeklyReportResponse> SendWeeklyReportAsync(Guid managerId, CancellationToken cancellationToken)
    {
        var result = await _weeklyReportService.SendAsync(managerId, cancellationToken);
        return result.ToResponse();
    }

    public async Task<ReportsSentResponse> SendDueReportsAsync(CancellationToken cancellationToken)
    {
        var sent = await _weeklyReportService.SendDueReportsAsync(cancellationToken);
        return new ReportsSentResponse(sent);
    }
}
