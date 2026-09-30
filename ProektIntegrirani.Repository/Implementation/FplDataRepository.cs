using EFCore.BulkExtensions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;

namespace ProektIntegrirani.Repository.Implementation;

// Rows are matched by primary key; the ETL builds Ids with GuidHelper, so an FPL record always maps to the same row.
public class FplDataRepository : IFplDataRepository
{
    private readonly ApplicationDbContext _context;

    public FplDataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task BulkInsertOrUpdateClubsAsync(List<Club> clubs)
    {
        await _context.BulkInsertOrUpdateAsync(clubs);
    }

    public async Task BulkInsertOrUpdateGameweeksAsync(List<Gameweek> gameweeks)
    {
        await _context.BulkInsertOrUpdateAsync(gameweeks);
    }

    public async Task BulkInsertOrUpdatePlayersAsync(List<Player> players)
    {
        await _context.BulkInsertOrUpdateAsync(players);
    }

    public async Task BulkInsertOrUpdateFixturesAsync(List<Fixture> fixtures)
    {
        await _context.BulkInsertOrUpdateAsync(fixtures);
    }
}
