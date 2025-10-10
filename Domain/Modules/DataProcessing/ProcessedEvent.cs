namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents a processed event from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Event model.
    /// </summary>
    public record ProcessedEvent
    {
        /// <summary>
        /// Gets the event title.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Gets the event start datetime.
        /// </summary>
        public DateTimeOffset? StartDateTime { get; init; }

        /// <summary>
        /// Gets the event end datetime.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; init; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public string? Location { get; init; }

        /// <summary>
        /// Gets the venue name.
        /// </summary>
        public string? Venue { get; init; }

        /// <summary>
        /// Gets the ticket link.
        /// </summary>
        public string? TicketLink { get; init; }

        /// <summary>
        /// Gets the source-specific event ID from Punchup.
        /// </summary>
        public string? SourceId { get; init; }
    }
}