using FplPlanner.Domain.Common;

namespace FplPlanner.Domain.Models;

// An external system allowed to call /api/external endpoints with an X-Api-Key header.
public class ApiClient : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // SHA-256 of the key; the key itself is shown only once, when the client is created.
    public string ApiKeyHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RequestsPerMinute { get; set; }

    public virtual ICollection<InboundSquadEntry> InboundSquadEntries { get; set; } = new List<InboundSquadEntry>();
}
