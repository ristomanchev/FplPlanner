namespace ProektIntegrirani.Web.Response;

public record FixtureResponse(
    Guid Id,
    int FplId,
    Guid? GameweekId,
    int? GameweekNumber,
    Guid HomeClubId,
    string HomeClubShortName,
    Guid AwayClubId,
    string AwayClubShortName,
    DateTime? KickoffTime,
    int? HomeScore,
    int? AwayScore,
    bool IsFinished);
