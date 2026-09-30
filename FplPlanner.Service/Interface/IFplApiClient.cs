using FplPlanner.Domain.ExternalModels;

namespace FplPlanner.Service.Interface;

public interface IFplApiClient
{
    Task<FplBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken = default);
    Task<List<FplFixture>> GetFixturesAsync(CancellationToken cancellationToken = default);
    Task<FplEntryResponse> GetEntryAsync(int entryId, CancellationToken cancellationToken = default);
    Task<FplPicksResponse> GetPicksAsync(int entryId, int gameweek, CancellationToken cancellationToken = default);
}
