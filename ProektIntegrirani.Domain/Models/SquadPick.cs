using ProektIntegrirani.Domain.Common;

namespace ProektIntegrirani.Domain.Models;

// Ternary relation Manager × Gameweek × Player.
public class SquadPick : BaseAuditableEntity
{
    // 1–11 starting XI, 12–15 bench (in substitution order).
    public int SquadPosition { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }

    public Guid ManagerId { get; set; }
    public virtual Manager Manager { get; set; } = null!;

    public Guid GameweekId { get; set; }
    public virtual Gameweek Gameweek { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
}
