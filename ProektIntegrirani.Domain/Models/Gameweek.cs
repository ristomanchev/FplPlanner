using ProektIntegrirani.Domain.Common;

namespace ProektIntegrirani.Domain.Models;

public class Gameweek : BaseEntity
{
    public int Number { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsFinished { get; set; }

    public virtual ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
    public virtual ICollection<SquadPick> SquadPicks { get; set; } = new List<SquadPick>();
    public virtual ICollection<PlayerPrediction> Predictions { get; set; } = new List<PlayerPrediction>();
}
