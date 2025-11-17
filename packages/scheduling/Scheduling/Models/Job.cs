using StageSide.Domain.Models;

namespace StageSide.Scheduling.Models
{
    public record Job : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid ScheduleId { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Created;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }

        public virtual Schedule Schedule { get; set; } = null!;
    }
}