namespace FplPlanner.Web.Response;

public record WeeklyReportResponse(
    Guid ManagerId,
    string TeamName,
    string? Email,
    int GameweekNumber,
    DateTime Deadline,
    SquadMemberResponse? Captain,
    SquadMemberResponse? ViceCaptain,
    TransferPlanResponse RecommendedTransfers,
    List<SquadMemberResponse> FlaggedPlayers,
    LineupResponse Lineup);

public record ReportsSentResponse(int Sent);
