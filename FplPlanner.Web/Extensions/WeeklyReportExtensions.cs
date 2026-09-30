using FplPlanner.Domain.Dto;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class WeeklyReportExtensions
{
    public static WeeklyReportResponse ToResponse(this WeeklyReportDto report)
    {
        return new WeeklyReportResponse(
            report.ManagerId,
            report.TeamName,
            report.Email,
            report.GameweekNumber,
            report.Deadline,
            report.Lineup.Captain?.ToResponse(),
            report.Lineup.ViceCaptain?.ToResponse(),
            report.TransferAdvice.RecommendedPlan.ToResponse(),
            report.FlaggedPlayers.Select(p => p.ToResponse()).ToList(),
            report.Lineup.ToResponse());
    }
}
