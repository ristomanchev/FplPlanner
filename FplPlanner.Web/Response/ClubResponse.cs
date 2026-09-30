namespace FplPlanner.Web.Response;

public record ClubResponse(
    Guid Id,
    int FplId,
    string Name,
    string ShortName,
    int StrengthHome,
    int StrengthAway);
