using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Domain.Dto;

public class PlayerDto
{
    public int FplId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string WebName { get; set; } = string.Empty;
    public Position Position { get; set; }
    public decimal Price { get; set; }
    public PlayerStatus Status { get; set; }
    public int? ChanceOfPlaying { get; set; }
    public string? News { get; set; }
    public Guid ClubId { get; set; }
}
