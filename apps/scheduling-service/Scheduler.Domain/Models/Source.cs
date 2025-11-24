using StageSide.Domain.Models;

namespace StageSide.Scheduler.Domain.Models;

public record Source : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Website { get; set; }
    public ICollection<Sku> Skus { get; set; } = []!;
}