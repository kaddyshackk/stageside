using StageSide.Pipeline.Domain.Models;

namespace StageSide.Pipeline.Domain.Scheduling.Models
{
    public record Execution : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid JobId { get; set; }
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Created;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual Job Job { get; set; } = null!;
    }
}