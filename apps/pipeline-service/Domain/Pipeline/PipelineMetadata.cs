namespace ComedyPull.Domain.Pipeline
{
    public record PipelineMetadata
    {
        public required string CollectionUrl { get; init; }
        public DateTimeOffset? CollectedAt { get; set; }
        public DateTimeOffset? TransformedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        
        public string? ContentHash { get; init; }
        public Dictionary<string, object> Tags { get; init; } = new();
    }
}