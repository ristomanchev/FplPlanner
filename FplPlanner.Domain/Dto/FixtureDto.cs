namespace FplPlanner.Domain.Dto;

public class FixtureDto
{
    public int FplId { get; set; }
    public Guid? GameweekId { get; set; }
    public Guid HomeClubId { get; set; }
    public Guid AwayClubId { get; set; }
    public DateTime? KickoffTime { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public bool IsFinished { get; set; }
}
