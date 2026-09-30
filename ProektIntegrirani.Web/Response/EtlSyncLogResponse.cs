namespace ProektIntegrirani.Web.Response;

public record EtlSyncLogResponse(
    Guid Id,
    string JobName,
    DateTime StartedAt,
    DateTime? CompletedAt,
    double? DurationSeconds,
    bool Success,
    string? ErrorMessage,
    int ClubsLoaded,
    int GameweeksLoaded,
    int PlayersLoaded,
    int FixturesLoaded,
    bool PredictionRecalculationQueued);
