namespace FplPlanner.Domain.Common;

// Who created / last changed the row and when; filled in by AuditInterceptor, never by hand.
public abstract class BaseAuditableEntity : BaseEntity
{
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }

    public string? LastModifiedBy { get; set; }
    public DateTime? DateLastModified { get; set; }
}
