using System.ComponentModel.DataAnnotations;

namespace FplPlanner.Web.Request;

public record ApiClientRequest(
    [Required, StringLength(100)] string Name,
    bool IsActive,
    [Range(1, 10000)] int RequestsPerMinute);
