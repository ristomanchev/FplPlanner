namespace FplPlanner.Domain.Dto;

public class ManagerDto
{
    public int FplEntryId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal Bank { get; set; }
    public int FreeTransfers { get; set; }
}
