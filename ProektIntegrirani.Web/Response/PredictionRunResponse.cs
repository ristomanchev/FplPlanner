namespace ProektIntegrirani.Web.Response;

public record PredictionRunResponse(
    List<int> GameweekNumbers,
    int PlayersEvaluated,
    int PredictionsSaved,
    DateTime CalculatedAt);
