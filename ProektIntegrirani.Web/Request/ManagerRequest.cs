using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Web.Request;

public record ManagerRequest(
    [Range(1, int.MaxValue)] int FplEntryId,
    [Required, StringLength(100)] string TeamName,
    [Required, StringLength(100)] string ManagerName,
    [EmailAddress] string? Email,
    [Range(0, 100)] decimal Bank,
    [Range(0, 5)] int FreeTransfers);
