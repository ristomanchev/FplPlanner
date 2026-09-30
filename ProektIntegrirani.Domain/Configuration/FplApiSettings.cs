namespace ProektIntegrirani.Domain.Configuration;

public class FplApiSettings
{
    public const string SectionName = "FplApi";

    public string BaseAddress { get; set; } = "https://fantasy.premierleague.com/api/";
    public int TimeoutSeconds { get; set; } = 30;

    // How long manager data (entry, picks) from the API is reused before it is requested again.
    public int CacheExpirationMinutes { get; set; } = 5;

    // How often the background job re-imports FPL data; 0 disables it.
    public int SyncIntervalHours { get; set; } = 6;
}
