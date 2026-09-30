using FplPlanner.Domain.Models;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class InboundSquadEntryExtensions
{
    public static InboundSquadEntryResponse ToResponse(this InboundSquadEntry entry)
    {
        return new InboundSquadEntryResponse(
            entry.Id,
            entry.ApiClientId,
            entry.ApiClient.Name,
            entry.Status.ToString(),
            entry.ReceivedAt,
            entry.ProcessedAt,
            entry.ManagerId,
            entry.ErrorMessage,
            entry.RawPayload);
    }

    public static List<InboundSquadEntryResponse> ToResponse(this IEnumerable<InboundSquadEntry> entries)
    {
        return entries.Select(e => e.ToResponse()).ToList();
    }
}
