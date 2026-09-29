using ProektIntegrirani.Domain.Common;

namespace ProektIntegrirani.Domain.Models;

public class Fixture : BaseEntity
{
    public int FplId { get; set; }
    public DateTime? KickoffTime { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public bool IsFinished { get; set; }

    // Null while the fixture is postponed and not yet rescheduled.
    public Guid? GameweekId { get; set; }
    public virtual Gameweek? Gameweek { get; set; }

    public Guid HomeClubId { get; set; }
    public virtual Club HomeClub { get; set; } = null!;

    public Guid AwayClubId { get; set; }
    public virtual Club AwayClub { get; set; } = null!;
}
