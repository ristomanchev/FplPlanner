namespace ProektIntegrirani.Web.Response;

public record SquadResponse(
    Guid ManagerId,
    string TeamName,
    int GameweekNumber,
    decimal Bank,
    decimal SquadValue,
    List<SquadMemberResponse> Members,
    SquadValidationResponse Validation);

public record SquadMemberResponse(
    Guid PlayerId,
    string WebName,
    string Position,
    string ClubShortName,
    decimal Price,
    string Status,
    int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain,
    decimal ExpectedPoints);

public record SquadValidationResponse(bool IsValid, List<string> Errors);

public record LineupResponse(
    int GameweekNumber,
    string Formation,
    List<SquadMemberResponse> StartingEleven,
    List<SquadMemberResponse> Bench,
    SquadMemberResponse? Captain,
    SquadMemberResponse? ViceCaptain,
    decimal ExpectedPoints);
