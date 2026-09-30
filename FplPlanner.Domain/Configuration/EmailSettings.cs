namespace FplPlanner.Domain.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "FPL Planner";
    public bool UseSsl { get; set; } = true;

    // The scheduled job queues the weekly reports once the next deadline is this close; 0 disables it.
    public int SendHoursBeforeDeadline { get; set; } = 24;
}
