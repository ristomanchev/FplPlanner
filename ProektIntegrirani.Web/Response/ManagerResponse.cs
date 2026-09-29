namespace ProektIntegrirani.Web.Response;

public record ManagerResponse(
    Guid Id,
    int FplEntryId,
    string TeamName,
    string ManagerName,
    string? Email,
    decimal Bank,
    int FreeTransfers,
    int? LastReportedGameweek);
