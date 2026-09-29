using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Web.Request;

public record FixtureRequest(
    [Range(1, int.MaxValue)] int FplId,
    Guid? GameweekId,
    [Required] Guid HomeClubId,
    [Required] Guid AwayClubId,
    DateTime? KickoffTime,
    [Range(0, 30)] int? HomeScore,
    [Range(0, 30)] int? AwayScore,
    bool IsFinished);
