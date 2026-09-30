namespace FplPlanner.Domain.ValueObjects;

// Expected points per FPL scoring category. Stored in the PlayerPredictions table (EF owned type).
public class PointsBreakdown
{
    public decimal Appearance { get; set; }
    public decimal Goals { get; set; }
    public decimal Assists { get; set; }
    public decimal CleanSheet { get; set; }
    public decimal GoalsConceded { get; set; }
    public decimal Saves { get; set; }
    public decimal DefensiveContribution { get; set; }
    public decimal Bonus { get; set; }
    public decimal Cards { get; set; }

    public decimal Total => Appearance + Goals + Assists + CleanSheet + GoalsConceded + Saves
                            + DefensiveContribution + Bonus + Cards;
}
