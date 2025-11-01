using Cronos;

namespace ComedyPull.Domain.Jobs.Services
{
    public static class CronCalculationService
    {
        public static DateTimeOffset? CalculateNextOccurence(string cronExpression)
        {
            return CronExpression.Parse(cronExpression).GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Utc);
        }
    }
}