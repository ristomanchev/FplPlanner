namespace ProektIntegrirani.Domain.Dto;

public class SquadPickDto
{
    public Guid ManagerId { get; set; }
    public Guid GameweekId { get; set; }
    public Guid PlayerId { get; set; }
    public int SquadPosition { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
}
