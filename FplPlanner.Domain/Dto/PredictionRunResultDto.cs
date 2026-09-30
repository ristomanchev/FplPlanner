namespace FplPlanner.Domain.Dto;

public class PredictionRunResultDto
{
    public List<int> GameweekNumbers { get; set; } = new();
    public int PlayersEvaluated { get; set; }
    public int PredictionsSaved { get; set; }
    public DateTime CalculatedAt { get; set; }
}
