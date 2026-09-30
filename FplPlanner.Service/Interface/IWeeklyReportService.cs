using FplPlanner.Domain.Dto;

namespace FplPlanner.Service.Interface;

public interface IWeeklyReportService
{
    Task<WeeklyReportDto> BuildAsync(Guid managerId);
    Task<string> RenderHtmlAsync(Guid managerId);
    // Puts the report e-mail on the queue; the gameweek is marked as reported once it has been sent.
    Task<WeeklyReportDto> SendAsync(Guid managerId, CancellationToken cancellationToken = default);

    // Queues the report for the next gameweek to every manager with an e-mail who has not received it yet.
    Task<int> SendDueReportsAsync(CancellationToken cancellationToken = default);
}
