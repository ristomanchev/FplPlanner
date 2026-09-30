using FplPlanner.Domain.Common;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.ValueObjects;

namespace FplPlanner.Domain.Models;

public class Player : BaseEntity
{
    public int FplId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string WebName { get; set; } = string.Empty;
    public Position Position { get; set; }
    public decimal Price { get; set; }
    public PlayerStatus Status { get; set; }
    public int? ChanceOfPlaying { get; set; }
    public string? News { get; set; }
    public PlayerSeasonStats Stats { get; set; } = new();

    public Guid ClubId { get; set; }
    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<SquadPick> SquadPicks { get; set; } = new List<SquadPick>();
    public virtual ICollection<PlayerPrediction> Predictions { get; set; } = new List<PlayerPrediction>();
}
