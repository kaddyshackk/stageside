using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Jobs
{
    public record Job : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Source Source { get; set; }
        public required Sku Sku { get; set; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public required DateTimeOffset NextExecution { get; set; }
        public DateTimeOffset? LastExecuted { get; set; }

        public ICollection<Sitemap> Sitemaps { get; set; } = [];
        public ICollection<Execution> Executions { get; set; } = [];
    }
}