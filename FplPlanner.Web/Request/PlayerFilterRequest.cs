using FplPlanner.Domain.Enums;

namespace FplPlanner.Web.Request;

public class PlayerFilterRequest : PaginateRequest
{
    public Position? Position { get; set; }
    public Guid? ClubId { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
}
