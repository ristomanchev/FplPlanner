namespace FplPlanner.Domain.Dto;

// What a manager submits when saving a whole squad (API or Excel import).
public class SquadPickInputDto
{
    public Guid PlayerId { get; set; }
    public int SquadPosition { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
}
