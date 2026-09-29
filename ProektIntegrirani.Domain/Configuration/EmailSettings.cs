namespace ProektIntegrirani.Domain.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public bool UseSsl { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "planner@fplplanner.local";
    public string FromName { get; set; } = "FPL Planner";

    // The scheduled job sends reports once the next deadline is this close; 0 disables it.
    public int SendHoursBeforeDeadline { get; set; } = 24;
}
