using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents a comedy event.
    /// </summary>
    public record Event : IAuditable, ITraceable
    {
        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the event title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the event start datetime.
        /// </summary>
        public required DateTimeOffset StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the event end datetime.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Gets the event comedian list.
        /// </summary>
        public virtual ICollection<Comedian> Comedians { get; init; } = new List<Comedian>();
        
        /// <summary>
        /// Gets the event venue.
        /// </summary>
        public virtual required Venue Venue { get; init; }
        
        // -- Auditable Fields ----
        
        /// <summary>
        /// Gets or sets the time the entity was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        public required string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        public required string UpdatedBy { get; set; }

        // -- Traceable Fields ----
        
        /// <summary>
        /// Gets or sets the source of the entity.
        /// </summary>
        public required DataSource Source { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was ingested.
        /// </summary>
        public required DateTimeOffset IngestedAt { get; set; }
    }
}
