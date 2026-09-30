using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

public static class ApiClientExtensions
{
    public static ApiClientResponse ToResponse(this ApiClient client)
    {
        return new ApiClientResponse(client.Id, client.Name, client.IsActive, client.RequestsPerMinute);
    }

    public static List<ApiClientResponse> ToResponse(this IEnumerable<ApiClient> clients)
    {
        return clients.Select(c => c.ToResponse()).ToList();
    }

    public static ApiClientWithKeyResponse ToResponse(this CreatedApiClientDto created)
    {
        var c = created.Client;
        return new ApiClientWithKeyResponse(c.Id, c.Name, c.IsActive, c.RequestsPerMinute, created.ApiKey);
    }

    public static ApiClientDto ToDto(this ApiClientRequest request)
    {
        return new ApiClientDto
        {
            Name = request.Name,
            IsActive = request.IsActive,
            RequestsPerMinute = request.RequestsPerMinute
        };
    }
}
