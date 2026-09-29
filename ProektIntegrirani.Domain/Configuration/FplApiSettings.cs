namespace ProektIntegrirani.Domain.Configuration;

public class FplApiSettings
{
    public const string SectionName = "FplApi";

    public string BaseAddress { get; set; } = "https://fantasy.premierleague.com/api/";
    public int TimeoutSeconds { get; set; } = 30;

    // How often the background job re-imports FPL data; 0 disables it.
    public int SyncIntervalHours { get; set; } = 6;
}
