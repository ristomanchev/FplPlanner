using FplPlanner.Domain.Models;
using FplPlanner.Service.Interface;
using FplPlanner.Web.Middlewares;

namespace FplPlanner.Web.Services;

// Lives in Web because it reads the HttpContext; the Service layer only knows ICurrentUser.
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string GetUserName()
    {
        var httpContext = _accessor.HttpContext;
        if (httpContext == null)
        {
            return "system";
        }

        return httpContext.Items[ApiKeyAuthMiddleware.ApiClientItemKey] is ApiClient client
            ? $"api-client:{client.Name}"
            : "api";
    }
}
