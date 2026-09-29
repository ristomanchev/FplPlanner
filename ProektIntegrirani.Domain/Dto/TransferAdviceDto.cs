namespace ProektIntegrirani.Domain.Dto;

public class TransferAdviceDto
{
    public Guid ManagerId { get; set; }
    public List<int> GameweekNumbers { get; set; } = new();
    public int FreeTransfers { get; set; }
    public decimal Bank { get; set; }

    // Objective (best XI + captain + small bench weight) of the current squad over the horizon.
    public decimal CurrentScore { get; set; }
    public List<TransferDto> BestSingleTransfers { get; set; } = new();
    public List<TransferPlanDto> Plans { get; set; } = new();
    public TransferPlanDto RecommendedPlan { get; set; } = new();
}

public class TransferDto
{
    public SquadMemberDto PlayerOut { get; set; } = new();
    public SquadMemberDto PlayerIn { get; set; } = new();
    public decimal Gain { get; set; }
    public decimal BankAfter { get; set; }
}

public class TransferPlanDto
{
    public List<TransferDto> Transfers { get; set; } = new();
    public decimal Gain { get; set; }
    public int HitCost { get; set; }
    public decimal NetGain => Gain - HitCost;
    public decimal BankAfter { get; set; }
}
