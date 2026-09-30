using System.Threading.RateLimiting;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Middlewares;

namespace FplPlanner.Web.Extensions;

public static class RateLimitingExtensions
{
    public const string ExternalApiPolicy = "external-api";
    private const int DefaultRequestsPerMinute = 60;

    public static IServiceCollection AddExternalApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // One fixed window per API key, sized by that client's RequestsPerMinute.
            options.AddPolicy(ExternalApiPolicy, context =>
            {
                var apiKey = context.Request.Headers[ApiKeyAuthMiddleware.ApiKeyHeader].ToString();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    apiKey = "missing-api-key";
                }

                var limit = context.Items[ApiKeyAuthMiddleware.ApiClientItemKey] is ApiClient { RequestsPerMinute: > 0 } client
                    ? client.RequestsPerMinute
                    : DefaultRequestsPerMinute;

                return RateLimitPartition.GetFixedWindowLimiter(apiKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
        });

        return services;
    }
}
