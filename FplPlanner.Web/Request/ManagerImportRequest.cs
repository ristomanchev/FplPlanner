using System.ComponentModel.DataAnnotations;

namespace FplPlanner.Web.Request;

public record ManagerImportRequest(
    [Range(1, int.MaxValue)] int FplEntryId,
    [EmailAddress] string? Email);
