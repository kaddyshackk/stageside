using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.Common;

namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents a batch of records being processed.
    /// </summary>
    public record Batch : BaseEntity
    {
        /// <summary>
        /// Gets the source of the entity.
        /// </summary>
        public DataSource Source { get; init; }
        
        /// <summary>
        /// Gets the data source type.
        /// </summary>
        public DataSourceType SourceType { get; init; }
        
        /// <summary>
        /// Gets or sets the current processing state.
        /// </summary>
        public ProcessingState State { get; set; } = ProcessingState.Transformed;
    }
}