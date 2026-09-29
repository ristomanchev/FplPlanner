namespace ProektIntegrirani.Domain.Messages;

// Published after a successful FPL ETL; the consumer recalculates predictions.
public record FplDataSyncedMessage(Guid MessageId, DateTime SyncedAt, int PlayersUpdated, int FixturesUpdated);
