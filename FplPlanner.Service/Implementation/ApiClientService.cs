using System.Security.Cryptography;
using System.Text;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

public class ApiClientService : IApiClientService
{
    private const int ApiKeyBytes = 32;

    private readonly IRepository<ApiClient> _repository;
    private readonly IRepository<InboundSquadEntry> _inboundRepository;

    public ApiClientService(IRepository<ApiClient> repository, IRepository<InboundSquadEntry> inboundRepository)
    {
        _repository = repository;
        _inboundRepository = inboundRepository;
    }

    public async Task<List<ApiClient>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            orderBy: x => x.OrderBy(c => c.Name));
        return result.ToList();
    }

    public async Task<ApiClient> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
                   selector: x => x,
                   predicate: x => x.Id == id)
               ?? throw new NotFoundException(nameof(ApiClient), id);
    }

    public async Task<ApiClient?> GetActiveByApiKeyAsync(string apiKey)
    {
        var hash = Hash(apiKey);
        return await _repository.GetAsync(
            selector: x => x,
            predicate: x => x.ApiKeyHash == hash && x.IsActive);
    }

    public async Task<CreatedApiClientDto> InsertAsync(ApiClientDto dto)
    {
        var apiKey = GenerateApiKey();
        var client = new ApiClient { ApiKeyHash = Hash(apiKey) };
        Apply(client, dto);

        await _repository.InsertAsync(client);
        return new CreatedApiClientDto { Client = client, ApiKey = apiKey };
    }

    public async Task<ApiClient> UpdateAsync(Guid id, ApiClientDto dto)
    {
        var client = await GetByIdAsync(id);
        Apply(client, dto);
        return await _repository.UpdateAsync(client);
    }

    // A lost key cannot be recovered (only its hash is stored), so a new one is issued.
    public async Task<CreatedApiClientDto> RegenerateKeyAsync(Guid id)
    {
        var client = await GetByIdAsync(id);
        var apiKey = GenerateApiKey();
        client.ApiKeyHash = Hash(apiKey);

        await _repository.UpdateAsync(client);
        return new CreatedApiClientDto { Client = client, ApiKey = apiKey };
    }

    public async Task<ApiClient> DeleteAsync(Guid id)
    {
        var client = await GetByIdAsync(id);

        if (await _inboundRepository.ExistsAsync(e => e.ApiClientId == id))
        {
            throw new BusinessRuleException(
                $"{client.Name} has sent squads; deactivate the client instead of deleting it.");
        }

        return await _repository.DeleteAsync(client);
    }

    private static string GenerateApiKey()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(ApiKeyBytes)).ToLowerInvariant();
    }

    // Keys are stored hashed, like passwords: a leaked database does not leak working keys.
    private static string Hash(string apiKey)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
    }

    private static void Apply(ApiClient client, ApiClientDto dto)
    {
        client.Name = dto.Name;
        client.IsActive = dto.IsActive;
        client.RequestsPerMinute = dto.RequestsPerMinute;
    }
}
