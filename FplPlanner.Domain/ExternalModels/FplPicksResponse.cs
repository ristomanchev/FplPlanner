using System.Text.Json.Serialization;

namespace FplPlanner.Domain.ExternalModels;

// GET /entry/{entryId}/event/{gameweek}/picks/
public class FplPicksResponse
{
    [JsonPropertyName("picks")] public List<FplPick> Picks { get; set; } = new();
}

public class FplPick
{
    [JsonPropertyName("element")] public int Element { get; set; }
    [JsonPropertyName("position")] public int Position { get; set; }
    [JsonPropertyName("is_captain")] public bool IsCaptain { get; set; }
    [JsonPropertyName("is_vice_captain")] public bool IsViceCaptain { get; set; }
}
