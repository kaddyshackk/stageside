using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Models
{
    public class Venue : IAuditable, ITraceable
    {
        /// <summary>
        /// Gets or sets the comedian id.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the slug identifier.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Gets the list of events.
        /// </summary>
        public virtual ICollection<Event> Events { get; init; } = new List<Event>();
        
        // -- Auditable Fields ----
        
        /// <summary>
        /// Gets or sets the time the entity was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        public string UpdatedBy { get; set; }
        
        // -- Traceable Fields ----
        
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