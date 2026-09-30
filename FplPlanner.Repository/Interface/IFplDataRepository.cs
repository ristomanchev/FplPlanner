using FplPlanner.Domain.Models;

namespace FplPlanner.Repository.Interface;

// Bulk load for the FPL ETL: one statement per table instead of one per row.
public interface IFplDataRepository
{
    Task BulkInsertOrUpdateClubsAsync(List<Club> clubs);
    Task BulkInsertOrUpdateGameweeksAsync(List<Gameweek> gameweeks);
    Task BulkInsertOrUpdatePlayersAsync(List<Player> players);
    Task BulkInsertOrUpdateFixturesAsync(List<Fixture> fixtures);
}
