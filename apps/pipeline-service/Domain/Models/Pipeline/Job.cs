namespace ComedyPull.Domain.Models.Pipeline
{
    public record Job : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Source Source { get; set; }
        public required Sku Sku { get; set; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public DateTimeOffset? LastExecuted { get; set; }
        public DateTimeOffset? NextExecution { get; set; }

        // Navigation properties
        public virtual ICollection<JobSitemap> Sitemaps { get; set; } = [];
        public virtual ICollection<JobExecution> Executions { get; set; } = [];
    }
}