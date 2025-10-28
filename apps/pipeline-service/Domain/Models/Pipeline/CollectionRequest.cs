namespace ComedyPull.Domain.Models.Pipeline
{
    public record CollectionRequest
    {
        public required Guid BatchId { get; init; }
        public required CollectionType Type { get; init; }
        public required DataSource Source { get; init; }
        public required ContentSku Sku { get; init; }
        public required string Url { get; init; }
    }
}