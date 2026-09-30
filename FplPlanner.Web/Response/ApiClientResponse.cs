namespace FplPlanner.Web.Response;

public record ApiClientResponse(
    Guid Id,
    string Name,
    bool IsActive,
    int RequestsPerMinute);

// Returned only on create / key regeneration: the only time the plain key is visible.
public record ApiClientWithKeyResponse(
    Guid Id,
    string Name,
    bool IsActive,
    int RequestsPerMinute,
    string ApiKey);
