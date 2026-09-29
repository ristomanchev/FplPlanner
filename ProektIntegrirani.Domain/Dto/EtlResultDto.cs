namespace ProektIntegrirani.Domain.Dto;

public class EtlResultDto
{
    public int ClubsInserted { get; set; }
    public int ClubsUpdated { get; set; }
    public int GameweeksInserted { get; set; }
    public int GameweeksUpdated { get; set; }
    public int PlayersInserted { get; set; }
    public int PlayersUpdated { get; set; }
    public int FixturesInserted { get; set; }
    public int FixturesUpdated { get; set; }
    public bool PredictionRecalculationQueued { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}
