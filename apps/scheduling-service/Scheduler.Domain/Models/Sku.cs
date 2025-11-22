using StageSide.Domain.Models;

namespace StageSide.Scheduler.Domain.Models;

public record Sku : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid SourceId { get; set; }
    public string Name { get; set; }
    public required SkuType Type { get; set; }
    
    // Navigation properties
    public ICollection<Schedule> Schedules { get; set; } = []!;
}