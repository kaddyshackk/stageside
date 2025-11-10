namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Defines a relationship between comedian and event.
    /// </summary>
    public record EventAct : AuditableEntity
    {
        /// <summary>
        /// ID of the comedian at the event.
        /// </summary>
        public required Guid ActId { get; set; }

        /// <summary>
        /// ID of the event.
        /// </summary>
        public required Guid EventId { get; set; }

        /// <summary>
        /// Navigation property to comedian.
        /// </summary>
        public virtual Act Act { get; set; } = null!;

        /// <summary>
        /// Navigation property to event.
        /// </summary>
        public virtual Event Event { get; set; } = null!;
    }
}