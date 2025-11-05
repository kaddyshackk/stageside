namespace ComedyPull.Domain.Jobs.Operations.CreateJob
{
    public record CreateJobCommand
    {
        public required string Source { get; init; }
        public required string Sku { get; init; }
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
        public ICollection<string>? SitemapUrls { get; init; }
    }
}