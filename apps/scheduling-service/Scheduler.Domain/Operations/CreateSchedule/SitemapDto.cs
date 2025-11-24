namespace StageSide.Scheduler.Domain.Operations.CreateSchedule
{
    public class SitemapDto
    {
        public required string Url { get; init; }
        public string? RegexFilter  { get; init; }
    }
}