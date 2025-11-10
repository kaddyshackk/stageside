namespace ComedyPull.Domain.Operations
{
    public record CreateJobCommand
    {
        public required string Source { get; init; }
        public required string Sku { get; init; }
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
        public ICollection<SitemapDto>? Sitemaps { get; init; }
    }
}