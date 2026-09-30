using FplPlanner.Domain.Enums;

namespace FplPlanner.Domain.Dto;

public class PlayerFilterDto
{
    public Position? Position { get; set; }
    public Guid? ClubId { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
}
