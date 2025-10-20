using ComedyPull.Domain.Modules.Common;

namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents a record to be processed.
    /// </summary>
    public record BronzeRecord : BaseEntity
    {
        /// <summary>
        /// Gets the processing batch id.
        /// </summary>
        public required Guid BatchId { get; init; }

        /// <summary>
        /// Gets or sets the current processing status.
        /// </summary>
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Processing;

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