namespace FplPlanner.Domain.Dto;

public class SquadValidationResultDto
{
    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
}
