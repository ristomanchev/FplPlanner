using System.ComponentModel.DataAnnotations;
using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Web.Request;

public record PlayerPredictionRequest(
    [Required] Guid PlayerId,
    [Required] Guid GameweekId,
    [EnumDataType(typeof(PredictionModelType))] PredictionModelType ModelType,
    [Range(0, 180)] decimal ExpectedMinutes,
    [Required] PointsBreakdownRequest Breakdown);

public record PointsBreakdownRequest(
    decimal Appearance,
    decimal Goals,
    decimal Assists,
    decimal CleanSheet,
    decimal GoalsConceded,
    decimal Saves,
    decimal DefensiveContribution,
    decimal Bonus,
    decimal Cards);
