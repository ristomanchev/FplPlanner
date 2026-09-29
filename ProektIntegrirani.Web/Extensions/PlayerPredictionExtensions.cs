using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Domain.ValueObjects;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

public static class PlayerPredictionExtensions
{
    public static PlayerPredictionResponse ToResponse(this PlayerPrediction prediction)
    {
        return new PlayerPredictionResponse(
            prediction.Id,
            prediction.PlayerId,
            prediction.Player.WebName,
            prediction.Player.Position.ToString(),
            prediction.Player.Club.ShortName,
            prediction.Player.Price,
            prediction.GameweekId,
            prediction.Gameweek.Number,
            prediction.ModelType.ToString(),
            prediction.ExpectedMinutes,
            prediction.ExpectedPoints,
            prediction.Breakdown.ToResponse(),
            prediction.CalculatedAt);
    }

    public static List<PlayerPredictionResponse> ToResponse(this IEnumerable<PlayerPrediction> predictions)
    {
        return predictions.Select(p => p.ToResponse()).ToList();
    }

    public static PointsBreakdownResponse ToResponse(this PointsBreakdown b)
    {
        return new PointsBreakdownResponse(b.Appearance, b.Goals, b.Assists, b.CleanSheet, b.GoalsConceded,
            b.Saves, b.DefensiveContribution, b.Bonus, b.Cards);
    }

    public static PlayerPredictionDto ToDto(this PlayerPredictionRequest request)
    {
        var b = request.Breakdown;
        return new PlayerPredictionDto
        {
            PlayerId = request.PlayerId,
            GameweekId = request.GameweekId,
            ModelType = request.ModelType,
            ExpectedMinutes = request.ExpectedMinutes,
            Breakdown = new PointsBreakdown
            {
                Appearance = b.Appearance,
                Goals = b.Goals,
                Assists = b.Assists,
                CleanSheet = b.CleanSheet,
                GoalsConceded = b.GoalsConceded,
                Saves = b.Saves,
                DefensiveContribution = b.DefensiveContribution,
                Bonus = b.Bonus,
                Cards = b.Cards
            }
        };
    }
}
