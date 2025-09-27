namespace ComedyPull.Api.Options
{
    public class ApiOptions
    {
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryAttempts { get; set; } = 3;
    }
}