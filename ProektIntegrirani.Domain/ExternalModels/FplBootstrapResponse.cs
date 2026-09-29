using System.Text.Json.Serialization;

namespace ProektIntegrirani.Domain.ExternalModels;

// GET /bootstrap-static/ — only the fields this application uses.
public class FplBootstrapResponse
{
    [JsonPropertyName("teams")] public List<FplTeam> Teams { get; set; } = new();
    [JsonPropertyName("events")] public List<FplEvent> Events { get; set; } = new();
    [JsonPropertyName("elements")] public List<FplElement> Elements { get; set; } = new();
}

public class FplTeam
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("short_name")] public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("strength_overall_home")] public int? StrengthOverallHome { get; set; }
    [JsonPropertyName("strength_overall_away")] public int? StrengthOverallAway { get; set; }
}

public class FplEvent
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("deadline_time")] public DateTime DeadlineTime { get; set; }
    [JsonPropertyName("finished")] public bool Finished { get; set; }
}

// FPL calls players "elements". Several numeric stats are sent as strings ("0.45").
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class FplElement
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("second_name")] public string SecondName { get; set; } = string.Empty;
    [JsonPropertyName("web_name")] public string WebName { get; set; } = string.Empty;
    [JsonPropertyName("element_type")] public int ElementType { get; set; }
    [JsonPropertyName("team")] public int Team { get; set; }
    [JsonPropertyName("now_cost")] public int NowCost { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "a";
    [JsonPropertyName("chance_of_playing_next_round")] public int? ChanceOfPlayingNextRound { get; set; }
    [JsonPropertyName("news")] public string? News { get; set; }

    [JsonPropertyName("total_points")] public int TotalPoints { get; set; }
    [JsonPropertyName("minutes")] public int Minutes { get; set; }
    [JsonPropertyName("starts")] public int Starts { get; set; }
    [JsonPropertyName("goals_scored")] public int GoalsScored { get; set; }
    [JsonPropertyName("assists")] public int Assists { get; set; }
    [JsonPropertyName("expected_goals")] public decimal ExpectedGoals { get; set; }
    [JsonPropertyName("expected_assists")] public decimal ExpectedAssists { get; set; }
    [JsonPropertyName("expected_goals_conceded")] public decimal ExpectedGoalsConceded { get; set; }
    [JsonPropertyName("saves")] public int Saves { get; set; }
    [JsonPropertyName("bonus")] public int Bonus { get; set; }
    [JsonPropertyName("yellow_cards")] public int YellowCards { get; set; }
    [JsonPropertyName("defensive_contribution")] public int DefensiveContribution { get; set; }
    [JsonPropertyName("team_join_date")] public DateOnly? TeamJoinDate { get; set; }
}
