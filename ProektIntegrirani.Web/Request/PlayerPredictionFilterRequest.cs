using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Web.Request;

public class PlayerPredictionFilterRequest : PaginateRequest
{
    public int? GameweekNumber { get; set; }
    public Position? Position { get; set; }
}
