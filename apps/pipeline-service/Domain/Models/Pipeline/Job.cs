namespace ComedyPull.Domain.Models.Pipeline
{
    public record Job : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Source Source { get; set; }
        public required Sku Sku { get; set; }
        public required string Name { get; set; }
        public required string CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public int MaxConcurrency { get; set; } = 1;
        public int TimeoutMinutes { get; set; } = 60;
        public DateTime? LastExecuted { get; set; }
        public DateTime? NextExecution { get; set; }

        // Navigation properties
        public virtual ICollection<JobSitemap> Sitemaps { get; set; } = [];
        public virtual ICollection<JobExecution> Executions { get; set; } = [];
    }
}