namespace ProektIntegrirani.Domain.Dto;

public class SquadDto
{
    public Guid ManagerId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int GameweekNumber { get; set; }
    public decimal Bank { get; set; }
    public decimal SquadValue { get; set; }
    public List<SquadMemberDto> Members { get; set; } = new();
    public SquadValidationResultDto Validation { get; set; } = new();
}
