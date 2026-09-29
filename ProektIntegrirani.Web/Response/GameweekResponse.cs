namespace ProektIntegrirani.Web.Response;

public record GameweekResponse(
    Guid Id,
    int Number,
    DateTime Deadline,
    bool IsFinished);
