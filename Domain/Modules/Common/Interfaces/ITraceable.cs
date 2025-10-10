using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Modules.Common.Interfaces
{
    /// <summary>
    /// Represents an entity that can be traced to an external source.
    /// </summary>
    public interface ITraceable
    {
        /// <summary>
        /// Gets or sets the source of the entity.
        /// </summary>
        DataSource Source { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was ingested.
        /// </summary>
        DateTimeOffset IngestedAt { get; set; }
    }
}