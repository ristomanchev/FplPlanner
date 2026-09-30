using FplPlanner.Domain.Enums;
using FplPlanner.Domain.ValueObjects;

namespace FplPlanner.Domain.Dto;

public class PlayerPredictionDto
{
    public Guid PlayerId { get; set; }
    public Guid GameweekId { get; set; }
    public PredictionModelType ModelType { get; set; }
    public decimal ExpectedMinutes { get; set; }
    public PointsBreakdown Breakdown { get; set; } = new();
}
