using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Jobs
{
    public record JobExecution : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid JobId { get; set; }
        public JobExecutionStatus Status { get; set; } = JobExecutionStatus.Executed;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual Job Job { get; set; } = null!;
    }
}