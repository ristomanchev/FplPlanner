namespace FplPlanner.Domain.Configuration;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string FplDataSyncedQueue { get; set; } = "fpl.data-synced";
    public int ReconnectDelaySeconds { get; set; } = 30;
}
