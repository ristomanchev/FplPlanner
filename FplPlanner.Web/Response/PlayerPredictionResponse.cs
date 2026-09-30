namespace FplPlanner.Web.Response;

public record PlayerPredictionResponse(
    Guid Id,
    Guid PlayerId,
    string PlayerWebName,
    string PlayerPosition,
    string ClubShortName,
    decimal PlayerPrice,
    Guid GameweekId,
    int GameweekNumber,
    string ModelType,
    decimal ExpectedMinutes,
    decimal ExpectedPoints,
    PointsBreakdownResponse Breakdown,
    DateTime CalculatedAt);

public record PointsBreakdownResponse(
    decimal Appearance,
    decimal Goals,
    decimal Assists,
    decimal CleanSheet,
    decimal GoalsConceded,
    decimal Saves,
    decimal DefensiveContribution,
    decimal Bonus,
    decimal Cards);
