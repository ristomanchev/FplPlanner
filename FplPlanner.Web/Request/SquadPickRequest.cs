using System.ComponentModel.DataAnnotations;

namespace FplPlanner.Web.Request;

public record SquadPickRequest(
    [Required] Guid ManagerId,
    [Required] Guid GameweekId,
    [Required] Guid PlayerId,
    [Range(1, 15)] int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain);
