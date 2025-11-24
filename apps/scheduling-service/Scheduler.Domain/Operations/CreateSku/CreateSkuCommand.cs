using StageSide.Domain.Models;

namespace StageSide.Scheduler.Domain.Operations.CreateSku;

public class CreateSkuCommand
{
    public required Guid SourceId { get; set; }
    public required string Name { get; set; }
    public required SkuType Type { get; set; }
}