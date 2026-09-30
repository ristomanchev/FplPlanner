using System.ComponentModel.DataAnnotations;

namespace FplPlanner.Web.Request;

public record ClubRequest(
    [Range(1, int.MaxValue)] int FplId,
    [Required, StringLength(100)] string Name,
    [Required, StringLength(3, MinimumLength = 3)] string ShortName,
    [Range(1, 5)] int StrengthHome,
    [Range(1, 5)] int StrengthAway);
