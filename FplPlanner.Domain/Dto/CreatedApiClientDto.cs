using FplPlanner.Domain.Models;

namespace FplPlanner.Domain.Dto;

// The plain API key exists only here, right after creation; the database keeps its hash.
public class CreatedApiClientDto
{
    public ApiClient Client { get; set; } = null!;
    public string ApiKey { get; set; } = string.Empty;
}
