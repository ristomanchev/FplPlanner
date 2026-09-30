namespace FplPlanner.Web.Response;

public record SquadPickResponse(
    Guid Id,
    Guid ManagerId,
    string ManagerTeamName,
    Guid GameweekId,
    int GameweekNumber,
    Guid PlayerId,
    string PlayerWebName,
    string PlayerPosition,
    string ClubShortName,
    decimal PlayerPrice,
    int SquadPosition,
    bool IsBench,
    bool IsCaptain,
    bool IsViceCaptain);
