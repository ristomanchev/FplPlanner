using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Web.Request;

// Full squad; the FPL rules (15 players, 2/5/5/3, ...) are checked by the service, which lists every violation.
public record SaveSquadRequest([Required] List<SquadPickInputRequest> Picks);

public record SquadPickInputRequest(
    [Required] Guid PlayerId,
    [Range(1, 15)] int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain);
