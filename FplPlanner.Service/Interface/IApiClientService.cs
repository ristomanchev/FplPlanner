using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IApiClientService
{
    Task<List<ApiClient>> GetAllAsync();
    Task<ApiClient> GetByIdAsync(Guid id);
    Task<ApiClient?> GetActiveByApiKeyAsync(string apiKey);
    Task<CreatedApiClientDto> InsertAsync(ApiClientDto dto);
    Task<ApiClient> UpdateAsync(Guid id, ApiClientDto dto);
    Task<CreatedApiClientDto> RegenerateKeyAsync(Guid id);
    Task<ApiClient> DeleteAsync(Guid id);
}
