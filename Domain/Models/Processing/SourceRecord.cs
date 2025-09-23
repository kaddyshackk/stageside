using ComedyPull.Domain.Enums;
using Newtonsoft.Json.Linq;

namespace ComedyPull.Domain.Models.Processing
{
    /// <summary>
    /// Represents a record to be processed.
    /// </summary>
    public record SourceRecord : TraceableEntity
    {
        /// <summary>
        /// Gets the processing batch id.
        /// </summary>
        public required string BatchId { get; init; }
        
        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public required EntityType EntityType { get; init; }
        
        /// <summary>
        /// Gets the record type.
        /// </summary>
        public required RecordType RecordType { get; init; }

        /// <summary>
        /// Gets or sets the current processing status.
        /// </summary>
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Processing;

        /// <summary>
        /// Gets or sets the current processing stage.
        /// </summary>
        public ProcessingStage Stage { get; set; } = ProcessingStage.Ingested;
        
        /// <summary>
        /// Gets the raw scraped data.
        /// </summary>
        public required string RawData { get; init; }
        
        /// <summary>
        /// Gets the latest processed data.
        /// </summary>
        public string? ProcessedData { get; set; }
        
        /// <summary>
        /// Gets the dedupe content hash.
        /// </summary>
        public string? ContentHash { get; set; }
    }
}