using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.ExternalModels;
using ProektIntegrirani.Domain.ValueObjects;

namespace ProektIntegrirani.Service.Implementation;

// The "Transform" step of the ETL: FPL API codes and units -> domain values.
public static class FplTransformations
{
    public static Position ToPosition(int elementType)
    {
        return elementType switch
        {
            1 => Position.Goalkeeper,
            2 => Position.Defender,
            3 => Position.Midfielder,
            4 => Position.Forward,
            _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, "Unknown FPL element type.")
        };
    }

    public static PlayerStatus ToPlayerStatus(string status)
    {
        return status switch
        {
            "a" => PlayerStatus.Available,
            "d" => PlayerStatus.Doubtful,
            "i" => PlayerStatus.Injured,
            "s" => PlayerStatus.Suspended,
            "u" => PlayerStatus.Unavailable,
            "n" => PlayerStatus.NotInSquad,
            _ => PlayerStatus.Unavailable
        };
    }

    // FPL sends money in tenths of a million: now_cost 105 = £10.5m.
    public static decimal ToMillions(int tenths)
    {
        return tenths / 10m;
    }

    public static string? ToNews(string? news)
    {
        return string.IsNullOrWhiteSpace(news) ? null : news.Trim();
    }

    // Older seasons had a 1–5 strength; keep a neutral 3 when the API leaves it out.
    public static int ToStrength(int? strength)
    {
        return strength is >= 1 and <= 5 ? strength.Value : 3;
    }

    public static PlayerSeasonStats ToStats(FplElement element)
    {
        return new PlayerSeasonStats
        {
            TotalPoints = element.TotalPoints,
            Minutes = element.Minutes,
            Starts = element.Starts,
            GoalsScored = element.GoalsScored,
            Assists = element.Assists,
            ExpectedGoals = element.ExpectedGoals,
            ExpectedAssists = element.ExpectedAssists,
            ExpectedGoalsConceded = element.ExpectedGoalsConceded,
            Saves = element.Saves,
            Bonus = element.Bonus,
            YellowCards = element.YellowCards,
            DefensiveContribution = element.DefensiveContribution,
            ClubJoinDate = element.TeamJoinDate
        };
    }
}
