using ComedyPull.Domain.Enums;

namespace ComedyPull.Domain.Modules.Common
{
    /// <summary>
    /// Defines a comedy event.
    /// </summary>
    public record Event : TraceableEntity
    {
        /// <summary>
        /// Gets the event title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug identifier for the event.
        /// Generated from comedian slug, venue slug, and date (e.g., "joe-list-comedy-store-2024-10-15").
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Gets the event status.
        /// </summary>
        public required EventStatus Status { get; set; }

        /// <summary>
        /// Gets the event start datetime.
        /// </summary>
        public required DateTimeOffset StartDateTime { get; set; }

        /// <summary>
        /// Gets the event end datetime.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Gets the venue foreign key.
        /// </summary>
        public required string VenueId { get; set; }

        /// <summary>
        /// Gets the event venue.
        /// </summary>
        public virtual required Venue Venue { get; set; }

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
