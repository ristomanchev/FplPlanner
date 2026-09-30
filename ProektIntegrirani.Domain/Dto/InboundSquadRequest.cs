using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Domain.Dto;

// Payload an external system sends to POST /api/external/squads (identified by FPL ids, not our Guids).
public record InboundSquadRequest(
    [Range(1, int.MaxValue)] int FplEntryId,
    [Range(1, 38)] int GameweekNumber,
    [Required, MinLength(1)] List<InboundSquadPick> Picks);

public record InboundSquadPick(
    [Range(1, int.MaxValue)] int FplId,
    [Range(1, 15)] int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain);
