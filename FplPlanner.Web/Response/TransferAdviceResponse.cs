namespace FplPlanner.Web.Response;

public record TransferAdviceResponse(
    Guid ManagerId,
    List<int> GameweekNumbers,
    int FreeTransfers,
    decimal Bank,
    decimal CurrentScore,
    TransferPlanResponse RecommendedPlan,
    List<TransferPlanResponse> Plans,
    List<TransferResponse> BestSingleTransfers);

public record TransferPlanResponse(
    int TransferCount,
    List<TransferResponse> Transfers,
    decimal Gain,
    int HitCost,
    decimal NetGain,
    decimal BankAfter);

public record TransferResponse(
    SquadMemberResponse PlayerOut,
    SquadMemberResponse PlayerIn,
    decimal Gain,
    decimal BankAfter);
