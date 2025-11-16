namespace StageSide.Domain.Models
{
    /// <summary>
    /// Defines a venue for comedy events.
    /// </summary>
    public record Venue : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the comedian id.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the venue name.
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
    }
}