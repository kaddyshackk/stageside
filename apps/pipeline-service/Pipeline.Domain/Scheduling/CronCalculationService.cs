using Cronos;

namespace StageSide.Pipeline.Domain.Scheduling
{
    public static class CronCalculationService
    {
        public static DateTimeOffset? CalculateNextOccurence(string cronExpression)
        {
            return CronExpression.Parse(cronExpression).GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
        }
    }
}