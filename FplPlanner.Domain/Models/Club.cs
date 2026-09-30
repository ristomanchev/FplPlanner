using FplPlanner.Domain.Common;

namespace FplPlanner.Domain.Models;

public class Club : BaseEntity
{
    public int FplId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    // FPL strength rating (1–5), used as the prior for the prediction model.
    public int StrengthHome { get; set; }
    public int StrengthAway { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    public virtual ICollection<Fixture> HomeFixtures { get; set; } = new List<Fixture>();
    public virtual ICollection<Fixture> AwayFixtures { get; set; } = new List<Fixture>();
}
