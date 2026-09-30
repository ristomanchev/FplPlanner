using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class FixtureExtensions
{
    public static FixtureResponse ToResponse(this Fixture fixture)
    {
        return new FixtureResponse(
            fixture.Id,
            fixture.FplId,
            fixture.GameweekId,
            fixture.Gameweek?.Number,
            fixture.HomeClubId,
            fixture.HomeClub.ShortName,
            fixture.AwayClubId,
            fixture.AwayClub.ShortName,
            fixture.KickoffTime,
            fixture.HomeScore,
            fixture.AwayScore,
            fixture.IsFinished);
    }

    public static List<FixtureResponse> ToResponse(this IEnumerable<Fixture> fixtures)
    {
        return fixtures.Select(f => f.ToResponse()).ToList();
    }

    public static FixtureDto ToDto(this FixtureRequest request)
    {
        return new FixtureDto
        {
            FplId = request.FplId,
            GameweekId = request.GameweekId,
            HomeClubId = request.HomeClubId,
            AwayClubId = request.AwayClubId,
            KickoffTime = request.KickoffTime?.ToUniversalTime(),
            HomeScore = request.HomeScore,
            AwayScore = request.AwayScore,
            IsFinished = request.IsFinished
        };
    }
}
