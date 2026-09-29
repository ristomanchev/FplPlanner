using ProektIntegrirani.Domain.Models;

namespace ProektIntegrirani.Service.Interface;

public interface IFplManagerImportService
{
    // Creates or updates the manager from FPL and imports the squad of their latest gameweek.
    Task<Manager> ImportAsync(int fplEntryId, string? email, CancellationToken cancellationToken = default);
}
