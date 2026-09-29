using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Domain.Dto;

// One player in a squad, with the data the squad rules and the optimizer need.
public class SquadMemberDto
{
    public Guid PlayerId { get; set; }
    public string WebName { get; set; } = string.Empty;
    public Position Position { get; set; }
    public Guid ClubId { get; set; }
    public string ClubShortName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PlayerStatus Status { get; set; }
    public int SquadPosition { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
    public decimal ExpectedPoints { get; set; }
}
