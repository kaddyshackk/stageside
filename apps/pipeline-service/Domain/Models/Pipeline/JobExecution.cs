namespace ComedyPull.Domain.Models.Pipeline
{
    public record JobExecution : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid JobId { get; set; }
        public JobExecutionStatus Status { get; set; } = JobExecutionStatus.Scheduled;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int ProcessedUrls { get; set; }
        public int TotalUrls { get; set; }

        // Navigation properties
        public virtual Job Job { get; set; } = null!;
        public virtual ICollection<PipelineContext> PipelineContexts { get; set; } = [];
    }
}