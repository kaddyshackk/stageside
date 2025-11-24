using StageSide.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.GetSource;

public class GetSourceSkuResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public required SkuType Type { get; set; }
}