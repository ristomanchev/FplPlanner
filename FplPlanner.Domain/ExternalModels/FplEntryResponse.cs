using System.Text.Json.Serialization;

namespace FplPlanner.Domain.ExternalModels;

// GET /entry/{entryId}/
public class FplEntryResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("player_first_name")] public string PlayerFirstName { get; set; } = string.Empty;
    [JsonPropertyName("player_last_name")] public string PlayerLastName { get; set; } = string.Empty;
    [JsonPropertyName("last_deadline_bank")] public int? LastDeadlineBank { get; set; }
    [JsonPropertyName("current_event")] public int? CurrentEvent { get; set; }
}
