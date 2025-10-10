namespace ComedyPull.Domain.Modules.Common
{
    /// <summary>
    /// Defined a comedian.
    /// </summary>
    public record Comedian : TraceableEntity
    {
        /// <summary>
        /// Gets or sets the comedian name.
        /// </summary>
        public required string Name { get; init; }
        
        /// <summary>
        /// Gets or sets the slug identifier.
        /// </summary>
        public required string Slug { get; init; }
        
        /// <summary>
        /// Gets or sets the comedian bio.
        /// </summary>
        public required string Bio { get; init; }
        
        /// <summary>
        /// Navigation property to ComedianEvent relationship table.
        /// </summary>
        public virtual ICollection<ComedianEvent> ComedianEvents { get; init; } = new List<ComedianEvent>();

        /// <summary>
        /// Navigation property to get comedians.
        /// </summary>
        public virtual ICollection<Event> Events => ComedianEvents.Select(ce => ce.Event).ToList();
    }
}
