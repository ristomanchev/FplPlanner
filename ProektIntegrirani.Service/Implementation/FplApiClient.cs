using System.Net;
using System.Net.Http.Json;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.ExternalModels;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

// Typed HttpClient for the public FPL API. Base address and timeout are configured in Program.cs.
public class FplApiClient : IFplApiClient
{
    private readonly HttpClient _httpClient;

    public FplApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<FplBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<FplBootstrapResponse>("bootstrap-static/", cancellationToken);
    }

    public Task<List<FplFixture>> GetFixturesAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<FplFixture>>("fixtures/", cancellationToken);
    }

    public Task<FplEntryResponse> GetEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        return GetAsync<FplEntryResponse>($"entry/{entryId}/", cancellationToken);
    }

    public Task<FplPicksResponse> GetPicksAsync(int entryId, int gameweek,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<FplPicksResponse>($"entry/{entryId}/event/{gameweek}/picks/", cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException($"FPL API resource '{path}' was not found.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
               ?? throw new InvalidOperationException($"FPL API returned an empty body for '{path}'.");
    }
}
