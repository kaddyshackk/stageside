using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Defines a comedy event.
    /// </summary>
    public record Event : TraceableEntity
    {
        /// <summary>
        /// Gets the event title.
        /// </summary>
        public required string Title { get; init; }
        
        /// <summary>
        /// Gets the event status.
        /// </summary>
        public required EventStatus Status { get; init; }

        /// <summary>
        /// Gets the event start datetime.
        /// </summary>
        public required DateTimeOffset StartDateTime { get; init; }

        /// <summary>
        /// Gets the event end datetime.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; init; }
        
        /// <summary>
        /// Gets the venue foreign key.
        /// </summary>
        public required string VenueId { get; init; }
        
        /// <summary>
        /// Gets the event venue.
        /// </summary>
        public virtual required Venue Venue { get; init; }
        
        /// <summary>
        /// Gets the Comedian relationship object.
        /// </summary>
        public virtual ICollection<ComedianEvent> ComedianEvents { get; init; } = new List<ComedianEvent>();

        /// <summary>
        /// Gets the event comedian list.
        /// </summary>
        public virtual ICollection<Comedian> Comedians => ComedianEvents.Select(ce => ce.Comedian).ToList();
    }
}
