namespace ComedyPull.Domain.Models
{
    public record BronzeRecord : TraceableEntity
    {
        public required string EntityType { get; init; }
        public required string ExternalId { get; init; }
        public required string RawData { get; init; }
        public bool Processed { get; init; }
        public DateTimeOffset ProcessedAt { get; init; }
    }
}