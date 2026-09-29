using Microsoft.AspNetCore.Mvc;
using ProektIntegrirani.Web.Mapper;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ReportMapper _reportMapper;

    public ReportsController(ReportMapper reportMapper)
    {
        _reportMapper = reportMapper;
    }

    [HttpGet("{managerId:guid}/weekly")]
    public async Task<ActionResult<WeeklyReportResponse>> GetWeeklyReport([FromRoute] Guid managerId)
    {
        return Ok(await _reportMapper.GetWeeklyReportAsync(managerId));
    }

    // The exact HTML that is e-mailed; open it in a browser to preview.
    [HttpGet("{managerId:guid}/weekly/html")]
    public async Task<ContentResult> GetWeeklyReportHtml([FromRoute] Guid managerId)
    {
        return Content(await _reportMapper.GetWeeklyReportHtmlAsync(managerId), "text/html");
    }

    [HttpPost("{managerId:guid}/weekly/send")]
    public async Task<ActionResult<WeeklyReportResponse>> SendWeeklyReport([FromRoute] Guid managerId,
        CancellationToken cancellationToken)
    {
        return Ok(await _reportMapper.SendWeeklyReportAsync(managerId, cancellationToken));
    }

    // Same as the scheduled job: every manager with an e-mail who has not had this gameweek's report.
    [HttpPost("weekly/send-due")]
    public async Task<ActionResult<ReportsSentResponse>> SendDueReports(CancellationToken cancellationToken)
    {
        return Ok(await _reportMapper.SendDueReportsAsync(cancellationToken));
    }
}
