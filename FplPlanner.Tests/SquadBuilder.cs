using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Enums;

namespace FplPlanner.Tests;

// Builds a valid 15-man squad (3-5-2, captain #7, vice #8) that tests then break on purpose.
public static class SquadBuilder
{
    public static List<SquadMemberDto> ValidSquad()
    {
        // Starting XI: GK, 3 DEF, 5 MID, 2 FWD; bench: GK, 2 DEF, 1 FWD. Five clubs, three players each.
        (Position Position, int SquadPosition)[] layout =
        [
            (Position.Goalkeeper, 1),
            (Position.Defender, 2), (Position.Defender, 3), (Position.Defender, 4),
            (Position.Midfielder, 5), (Position.Midfielder, 6), (Position.Midfielder, 7),
            (Position.Midfielder, 8), (Position.Midfielder, 9),
            (Position.Forward, 10), (Position.Forward, 11),
            (Position.Goalkeeper, 12), (Position.Defender, 13), (Position.Defender, 14), (Position.Forward, 15)
        ];

        var clubs = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        return layout.Select((slot, i) => new SquadMemberDto
        {
            PlayerId = Guid.NewGuid(),
            WebName = $"Player{i + 1}",
            Position = slot.Position,
            ClubId = clubs[i % clubs.Length],
            ClubShortName = $"C{i % clubs.Length}",
            Price = 5m,
            Status = PlayerStatus.Available,
            SquadPosition = slot.SquadPosition,
            IsCaptain = slot.SquadPosition == 7,
            IsViceCaptain = slot.SquadPosition == 8
        }).ToList();
    }
}
