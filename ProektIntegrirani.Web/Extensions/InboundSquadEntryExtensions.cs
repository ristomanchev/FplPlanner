using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

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
