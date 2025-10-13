using ComedyPull.Application.Modules.Public.Shared;
using ComedyPull.Domain.Enums;

namespace ComedyPull.Application.Modules.Public.Events.GetEventBySlug
{
    public record GetEventBySlugResponse
    {
        /// <summary>
        /// Gets the event title.
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Gets or sets the slug identifier for the event.
        /// </summary>
        public required string Slug { get; init; }
        
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
        /// Gets the event venue.
        /// </summary>
        public required VenueResponse Venue { get; init; }

        /// <summary>
        /// Gets the Comedian relationship object.
        /// </summary>
        public required List<ComedianResponse> Comedians { get; init; }
    }
}