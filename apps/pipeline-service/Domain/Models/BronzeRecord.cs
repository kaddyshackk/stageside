using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents a record to be processed.
    /// </summary>
    public record BronzeRecord : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the comedian id.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets the processing batch id.
        /// </summary>
        public required Guid BatchId { get; init; }

        /// <summary>
        /// Gets or sets the current processing status.
        /// </summary>
        public ProcessingState State { get; set; } = ProcessingState.Pending;

        public required ContentSku Sku { get; init; }
        
        /// <summary>
        /// Gets the raw source data.
        /// </summary>
        public required string Data { get; init; }
        
        /// <summary>
        /// Gets the dedupe content hash.
        /// </summary>
        public string? ContentHash { get; init; }
    }
}