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
        public required string ComedianId { get; set; }

        /// <summary>
        /// ID of the event.
        /// </summary>
        public required string EventId { get; set; }

        /// <summary>
        /// Navigation property to comedian.
        /// </summary>
        public virtual Comedian Comedian { get; set; } = null!;

        /// <summary>
        /// Navigation property to event.
        /// </summary>
        public virtual Event Event { get; set; } = null!;
    }
}