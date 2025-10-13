namespace ComedyPull.Domain.Modules.Common
{
    /// <summary>
    /// Defines a venue for comedy events.
    /// </summary>
    public record Venue : TraceableEntity
    {
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