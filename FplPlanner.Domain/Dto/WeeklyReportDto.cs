namespace FplPlanner.Domain.Dto;

public class WeeklyReportDto
{
    public Guid ManagerId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int GameweekNumber { get; set; }
    public DateTime Deadline { get; set; }
    public LineupDto Lineup { get; set; } = new();
    public TransferAdviceDto TransferAdvice { get; set; } = new();

    // Squad players who are injured, doubtful, suspended or unavailable.
    public List<SquadMemberDto> FlaggedPlayers { get; set; } = new();
}
