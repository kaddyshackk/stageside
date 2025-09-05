namespace ComedyPull.Api.Configuration;

public class ApiSettings
{
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
}