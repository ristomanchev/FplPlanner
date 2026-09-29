namespace ProektIntegrirani.Domain.ValueObjects;

// Season totals from the FPL API. Stored in the Players table (EF owned type);
// only the ETL writes them, CRUD requests never change them.
public class PlayerSeasonStats
{
    public int TotalPoints { get; set; }
    public int Minutes { get; set; }
    public int Starts { get; set; }
    public int GoalsScored { get; set; }
    public int Assists { get; set; }
    public decimal ExpectedGoals { get; set; }
    public decimal ExpectedAssists { get; set; }
    public decimal ExpectedGoalsConceded { get; set; }
    public int Saves { get; set; }
    public int Bonus { get; set; }
    public int YellowCards { get; set; }
    public int DefensiveContribution { get; set; }
}
