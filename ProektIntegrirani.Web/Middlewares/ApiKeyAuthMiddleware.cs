using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Web.Middlewares;

// Guards /api/external/*: the X-Api-Key header must belong to an active ApiClient.
public class ApiKeyAuthMiddleware
{
    public const string ApiKeyHeader = "X-Api-Key";
    public const string ApiClientItemKey = "ApiClient";

    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiClientService apiClientService)
    {
        if (!context.Request.Path.StartsWithSegments("/api/external"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Error = "Api Key is Required" });
            return;
        }

        var client = await apiClientService.GetActiveByApiKeyAsync(apiKey.ToString());
        if (client == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Error = "Api Key is not valid" });
            return;
        }

        context.Items[ApiClientItemKey] = client;
        await _next(context);
    }
}
