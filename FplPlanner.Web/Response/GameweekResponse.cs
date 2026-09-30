namespace FplPlanner.Web.Response;

public record GameweekResponse(
    Guid Id,
    int Number,
    DateTime Deadline,
    bool IsFinished);
