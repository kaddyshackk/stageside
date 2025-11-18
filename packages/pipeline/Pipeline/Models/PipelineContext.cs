using StageSide.Domain.Models;

namespace StageSide.Pipeline.Models
{
    public record PipelineContext
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid JobId { get; init; } = Guid.NewGuid();
        public required Source Source { get; init; }
        public required SkuKey SkuKey { get; init; }
        public required PipelineMetadata Metadata { get; init; }
        public string? RawData { get; set; }
        public ICollection<ProcessedEntity> ProcessedEntities { get; set; } = [];
        public ProcessingState State { get; set; } = ProcessingState.Pending;
        public string? ErrorMessage { get; set; }
    }
}