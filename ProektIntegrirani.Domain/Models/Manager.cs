using ProektIntegrirani.Domain.Common;

namespace ProektIntegrirani.Domain.Models;

public class Manager : BaseEntity
{
    public int FplEntryId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal Bank { get; set; }
    public int FreeTransfers { get; set; }

    public virtual ICollection<SquadPick> SquadPicks { get; set; } = new List<SquadPick>();
}
