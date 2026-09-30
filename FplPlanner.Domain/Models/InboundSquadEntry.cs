using FplPlanner.Domain.Common;
using FplPlanner.Domain.Enums;

namespace FplPlanner.Domain.Models;

// A squad pushed by an external system, stored as received and processed later in the background.
public class InboundSquadEntry : BaseEntity
{
    public string RawPayload { get; set; } = string.Empty;
    public InboundSquadStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public Guid ApiClientId { get; set; }
    public virtual ApiClient ApiClient { get; set; } = null!;

    // Set when processing succeeds: the manager whose squad was saved.
    public Guid? ManagerId { get; set; }
}
