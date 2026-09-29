using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Domain.Dto;

public class PlayerFilterDto
{
    public Position? Position { get; set; }
    public Guid? ClubId { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
}
