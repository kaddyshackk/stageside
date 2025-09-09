using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents an entity that can be traced to its source.
    /// </summary>
    public record TraceableEntity : BaseEntity, ITraceable
    {
        /// <summary>
        /// Gets or sets the source of the entity.
        /// </summary>
        public DataSource Source { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was ingested.
        /// </summary>
        public DateTimeOffset IngestedAt { get; set; }
    }
}