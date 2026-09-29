using ProektIntegrirani.Domain.Common;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.ValueObjects;

namespace ProektIntegrirani.Domain.Models;

// Expected points for a player in a gameweek, per prediction model.
public class PlayerPrediction : BaseEntity
{
    public PredictionModelType ModelType { get; set; }
    public decimal ExpectedMinutes { get; set; }

    // Stored (not computed) so predictions can be sorted and filtered in SQL.
    public decimal ExpectedPoints { get; set; }
    public PointsBreakdown Breakdown { get; set; } = new();
    public DateTime CalculatedAt { get; set; }

    public Guid PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;

    public Guid GameweekId { get; set; }
    public virtual Gameweek Gameweek { get; set; } = null!;
}
