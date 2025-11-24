namespace StageSide.Pipeline.Models
{
    /// <summary>
    /// Represents a processed event from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Event model.
    /// Each SilverRecord with EntityType.Event contains one ProcessedEvent.
    /// </summary>
    public record ProcessedEvent
    {
        /// <summary>
        /// Gets the event title.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Gets the event slug used for deduplication.
        /// Generated from comedian slug, venue slug, and date (e.g., "joe-list-comedy-store-2024-10-15").
        /// </summary>
        public string? Slug { get; init; }

        /// <summary>
        /// Gets the comedian slug for correlation.
        /// Used to link this event to the correct comedian during Complete stage.
        /// </summary>
        public string? ComedianSlug { get; init; }

        /// <summary>
        /// Gets the venue slug for correlation.
        /// Used to link this event to the correct venue during Complete stage.
        /// </summary>
        public string? VenueSlug { get; init; }

        /// <summary>
        /// Gets the event start datetime.
        /// </summary>
        public DateTimeOffset? StartDateTime { get; init; }

        /// <summary>
        /// Gets the event end datetime.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; init; }

        /// <summary>
        /// Gets the ticket link.
        /// </summary>
        public string? TicketLink { get; init; }
    }
}