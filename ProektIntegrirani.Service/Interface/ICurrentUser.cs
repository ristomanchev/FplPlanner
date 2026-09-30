namespace ProektIntegrirani.Service.Interface;

public interface ICurrentUser
{
    // "api-client:<name>" for external systems, "api" for other HTTP requests, "system" for background jobs.
    string GetUserName();
}
