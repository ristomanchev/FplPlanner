using FplPlanner.Domain.Enums;

namespace FplPlanner.Web.Request;

public class PlayerPredictionFilterRequest : PaginateRequest
{
    public int? GameweekNumber { get; set; }
    public Position? Position { get; set; }
}
