using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents a record that has been transformed to a consistent structure.
    /// </summary>
    public record SilverRecord : AuditableEntity
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
        /// Gets the original bronze record id.
        /// </summary>
        public required Guid BronzeRecordId { get; init; }
        
        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public required EntityType EntityType { get; init; }

        /// <summary>
        /// Gets or sets the current processing status.
        /// </summary>
        public ProcessingState State { get; set; } = ProcessingState.Pending;

        /// <summary>
        /// Gets the transformed data.
        /// </summary>
        public required string Data { get; set; }
        
        /// <summary>
        /// Gets the dedupe content hash.
        /// </summary>
        public string? ContentHash { get; set; }
    }
}