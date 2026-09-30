namespace FplPlanner.Domain.Dto;

// Best starting XI, bench order and captaincy for a squad, by expected points.
public class LineupDto
{
    public int GameweekNumber { get; set; }
    public List<SquadMemberDto> StartingEleven { get; set; } = new();
    public List<SquadMemberDto> Bench { get; set; } = new();
    public SquadMemberDto? Captain { get; set; }
    public SquadMemberDto? ViceCaptain { get; set; }

    // Starting XI with the captain counted twice.
    public decimal ExpectedPoints { get; set; }
}
