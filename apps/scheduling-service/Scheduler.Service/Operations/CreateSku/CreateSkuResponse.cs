using StageSide.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.CreateSku
{
    public class CreateSkuResponse
    {
        public required string Id { get; set; }
        public required string SourceId { get; set; }
        public required string Name { get; set; }
        public required SkuType Type { get; set; }
    }
}