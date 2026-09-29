using ProektIntegrirani.Domain.Common;
using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Domain.Models;

// Expected points for a player in a gameweek, per prediction model.
public class PlayerPrediction : BaseEntity
{
    public PredictionModelType ModelType { get; set; }
    public decimal ExpectedMinutes { get; set; }
    public decimal ExpectedPoints { get; set; }
    public DateTime CalculatedAt { get; set; }

    // Breakdown of ExpectedPoints by scoring category.
    public decimal AppearancePoints { get; set; }
    public decimal GoalPoints { get; set; }
    public decimal AssistPoints { get; set; }
    public decimal CleanSheetPoints { get; set; }
    public decimal GoalsConcededPoints { get; set; }
    public decimal SavePoints { get; set; }
    public decimal DefensiveContributionPoints { get; set; }
    public decimal BonusPoints { get; set; }
    public decimal CardPoints { get; set; }

    public Guid PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;

    public Guid GameweekId { get; set; }
    public virtual Gameweek Gameweek { get; set; } = null!;
}
