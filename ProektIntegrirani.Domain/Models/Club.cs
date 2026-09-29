using ProektIntegrirani.Domain.Common;

namespace ProektIntegrirani.Domain.Models;

public class Club : BaseEntity
{
    public int FplId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    public virtual ICollection<Fixture> HomeFixtures { get; set; } = new List<Fixture>();
    public virtual ICollection<Fixture> AwayFixtures { get; set; } = new List<Fixture>();
}
