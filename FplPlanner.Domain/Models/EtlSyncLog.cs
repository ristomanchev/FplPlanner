using FplPlanner.Domain.Common;

namespace FplPlanner.Domain.Models;

// One row per ETL run: when it ran, whether it succeeded and what it loaded.
public class EtlSyncLog : BaseEntity
{
    public string JobName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public int ClubsLoaded { get; set; }
    public int GameweeksLoaded { get; set; }
    public int PlayersLoaded { get; set; }
    public int FixturesLoaded { get; set; }
    public bool PredictionRecalculationQueued { get; set; }
}
