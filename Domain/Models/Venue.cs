namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Defines a venue for comedy events.
    /// </summary>
    public record Venue : TraceableEntity
    {
        /// <summary>
        /// Gets or sets the
        /// </summary>
        public required string Name { get; init; }
        
        /// <summary>
        /// Gets or sets the slug identifier.
        /// </summary>
        public required string Slug { get; init; }

        /// <summary>
        /// Gets the list of events.
        /// </summary>
        public virtual ICollection<Event> Events { get; init; } = new List<Event>();
    }
}