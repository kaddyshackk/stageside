namespace ComedyPull.Domain.Modules.Common
{
    /// <summary>
    /// Defines a relationship between comedian and event.
    /// </summary>
    public record ComedianEvent
    {
        /// <summary>
        /// ID of the comedian at the event.
        /// </summary>
        public required string ComedianId { get; init; }
        
        /// <summary>
        /// ID of the event.
        /// </summary>
        public required string EventId { get; init; }

        /// <summary>
        /// Navigation property to comedian.
        /// </summary>
        public virtual Comedian Comedian { get; init; } = null!;
        
        /// <summary>
        /// Navigation property to event.
        /// </summary>
        public virtual Event Event { get; init; } = null!;
    }
}