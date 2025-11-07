using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Pipeline
{
    public record ProcessedEntity
    {
        public required EntityType Type { get; init; }
        public required object Data { get; init; }
        public string? ContentHash { get; init; }
    }
}