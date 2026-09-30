using FplPlanner.Domain.Models;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class EtlExtensions
{
    public static EtlSyncLogResponse ToResponse(this EtlSyncLog log)
    {
        return new EtlSyncLogResponse(
            log.Id,
            log.JobName,
            log.StartedAt,
            log.CompletedAt,
            log.CompletedAt is { } completed ? Math.Round((completed - log.StartedAt).TotalSeconds, 2) : null,
            log.Success,
            log.ErrorMessage,
            log.ClubsLoaded,
            log.GameweeksLoaded,
            log.PlayersLoaded,
            log.FixturesLoaded,
            log.PredictionRecalculationQueued);
    }

    public static List<EtlSyncLogResponse> ToResponse(this IEnumerable<EtlSyncLog> logs)
    {
        return logs.Select(l => l.ToResponse()).ToList();
    }
}
