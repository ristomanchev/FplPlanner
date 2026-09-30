using FplPlanner.Domain.Enums;

namespace FplPlanner.Domain.Rules;

// Official FPL squad rules for 2026/27.
public static class FplRules
{
    public const int SquadSize = 15;
    public const int StartingElevenSize = 11;
    public const int MaxPlayersPerClub = 3;
    public const int TransferHitCost = 4;

    public static readonly IReadOnlyDictionary<Position, int> SquadShape = new Dictionary<Position, int>
    {
        [Position.Goalkeeper] = 2,
        [Position.Defender] = 5,
        [Position.Midfielder] = 5,
        [Position.Forward] = 3
    };

    public static readonly IReadOnlyDictionary<Position, (int Min, int Max)> StartingElevenLimits =
        new Dictionary<Position, (int Min, int Max)>
        {
            [Position.Goalkeeper] = (1, 1),
            [Position.Defender] = (3, 5),
            [Position.Midfielder] = (2, 5),
            [Position.Forward] = (1, 3)
        };

    public static bool IsStarter(int squadPosition) => squadPosition <= StartingElevenSize;
}
