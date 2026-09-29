using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

public static class EtlExtensions
{
    public static EtlResultResponse ToResponse(this EtlResultDto result)
    {
        return new EtlResultResponse(
            new EtlEntityCountResponse(result.ClubsInserted, result.ClubsUpdated),
            new EtlEntityCountResponse(result.GameweeksInserted, result.GameweeksUpdated),
            new EtlEntityCountResponse(result.PlayersInserted, result.PlayersUpdated),
            new EtlEntityCountResponse(result.FixturesInserted, result.FixturesUpdated),
            result.PredictionRecalculationQueued,
            result.StartedAt,
            Math.Round((result.FinishedAt - result.StartedAt).TotalSeconds, 2));
    }
}
