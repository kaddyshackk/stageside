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

        // Navigation properties
        public virtual ICollection<Sitemap> Sitemaps { get; set; } = [];
        public virtual ICollection<Execution> Executions { get; set; } = [];
    }
}