using StageSide.Domain.Models;

namespace StageSide.Scheduler.Domain.Models
{
    public record Schedule : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid SourceId { get; set; }
        public required Guid SkuId { get; init; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required DateTimeOffset NextExecution { get; set; }
        public DateTimeOffset? LastExecution { get; set; }
        public required bool IsActive { get; set; }

        public Source Source { get; set; } = null!;
        public Sku Sku { get; set; } = null!;
        public ICollection<Job> Jobs { get; set; } = [];
    }
}
