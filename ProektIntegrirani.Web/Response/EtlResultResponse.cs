namespace ProektIntegrirani.Web.Response;

public record EtlResultResponse(
    EtlEntityCountResponse Clubs,
    EtlEntityCountResponse Gameweeks,
    EtlEntityCountResponse Players,
    EtlEntityCountResponse Fixtures,
    DateTime StartedAt,
    double DurationSeconds);

public record EtlEntityCountResponse(int Inserted, int Updated);
