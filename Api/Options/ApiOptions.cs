namespace ComedyPull.Api.Configuration;

public class ApiOptions
{
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
}