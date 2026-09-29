using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Web.Request;

public record GameweekRequest(
    [Range(1, 38)] int Number,
    [Required] DateTime Deadline,
    bool IsFinished);
