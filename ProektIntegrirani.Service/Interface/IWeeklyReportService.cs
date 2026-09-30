using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

public interface IWeeklyReportService
{
    Task<WeeklyReportDto> BuildAsync(Guid managerId);
    Task<string> RenderHtmlAsync(Guid managerId);
    // Puts the report e-mail on the queue; EmailBackgroundService sends it.
    Task<WeeklyReportDto> SendAsync(Guid managerId, CancellationToken cancellationToken = default);

    // Queues the report for the next gameweek to every manager with an e-mail who has not received it yet.
    Task<int> SendDueReportsAsync(CancellationToken cancellationToken = default);
}
