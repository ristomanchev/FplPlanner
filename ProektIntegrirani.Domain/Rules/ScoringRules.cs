using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Domain.Rules;

// FPL points per event (game_config.scoring from bootstrap-static, 2026/27).
public static class ScoringRules
{
    public const int LongPlay = 2;          // 60+ minutes
    public const int ShortPlay = 1;         // 1–59 minutes
    public const int Assist = 3;
    public const int PerThreeSaves = 1;
    public const int BonusPoint = 1;
    public const int YellowCard = -1;

    public static int Goal(Position position) => position switch
    {
        Position.Goalkeeper => 10,
        Position.Defender => 6,
        Position.Midfielder => 5,
        _ => 4
    };

    public static int CleanSheet(Position position) => position switch
    {
        Position.Goalkeeper or Position.Defender => 4,
        Position.Midfielder => 1,
        _ => 0
    };

    // Per two goals conceded.
    public static int GoalsConceded(Position position) =>
        position is Position.Goalkeeper or Position.Defender ? -1 : 0;

    public static int DefensiveContribution(Position position) =>
        position == Position.Goalkeeper ? 0 : 2;

    // Actions needed for the defensive contribution points (not exposed by the API).
    public static int DefensiveContributionThreshold(Position position) =>
        position == Position.Defender ? 10 : 12;
}
