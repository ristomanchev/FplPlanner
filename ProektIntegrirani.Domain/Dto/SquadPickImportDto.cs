namespace ProektIntegrirani.Domain.Dto;

// One valid row of an imported squad sheet.
public class SquadPickImportDto
{
    public int FplId { get; set; }
    public Guid PlayerId { get; set; }
    public string WebName { get; set; } = string.Empty;
    public int SquadPosition { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
}
