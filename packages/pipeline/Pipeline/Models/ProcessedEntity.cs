using StageSide.Domain.Models;

namespace StageSide.Pipeline.Models
{
    public record ProcessedEntity
    {
        public required EntityType Type { get; init; }
        public required object Data { get; init; }
        public string? ContentHash { get; init; }
    }
}