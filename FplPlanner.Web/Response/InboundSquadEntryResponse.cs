namespace FplPlanner.Web.Response;

public record InboundSquadEntryResponse(
    Guid Id,
    Guid ApiClientId,
    string ApiClientName,
    string Status,
    DateTime ReceivedAt,
    DateTime? ProcessedAt,
    Guid? ManagerId,
    string? ErrorMessage,
    string RawPayload);
