namespace ProektIntegrirani.Web.Response;

public record PlayerResponse(
    Guid Id,
    int FplId,
    string FirstName,
    string LastName,
    string WebName,
    string Position,
    decimal Price,
    string Status,
    int? ChanceOfPlaying,
    string? News,
    Guid ClubId,
    string ClubShortName,
    PlayerStatsResponse Stats);

public record PlayerStatsResponse(
    int TotalPoints,
    int Minutes,
    int Starts,
    int GoalsScored,
    int Assists,
    decimal ExpectedGoals,
    decimal ExpectedAssists,
    int Saves,
    int Bonus,
    int DefensiveContribution);
