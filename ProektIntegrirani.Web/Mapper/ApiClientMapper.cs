using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Mapper;

public class ApiClientMapper
{
    private readonly IApiClientService _apiClientService;

    public ApiClientMapper(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<List<ApiClientResponse>> GetAllAsync()
    {
        var result = await _apiClientService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<ApiClientResponse> GetByIdAsync(Guid id)
    {
        var result = await _apiClientService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<ApiClientWithKeyResponse> InsertAsync(ApiClientRequest request)
    {
        var result = await _apiClientService.InsertAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<ApiClientResponse> UpdateAsync(Guid id, ApiClientRequest request)
    {
        var result = await _apiClientService.UpdateAsync(id, request.ToDto());
        return result.ToResponse();
    }

    public async Task<ApiClientWithKeyResponse> RegenerateKeyAsync(Guid id)
    {
        var result = await _apiClientService.RegenerateKeyAsync(id);
        return result.ToResponse();
    }

    public async Task<ApiClientResponse> DeleteAsync(Guid id)
    {
        var result = await _apiClientService.DeleteAsync(id);
        return result.ToResponse();
    }
}
