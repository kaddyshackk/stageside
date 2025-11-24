namespace StageSide.Pipeline.Models
{
    /// <summary>
    /// Represents a processed venue from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Venue model.
    /// Each SilverRecord with EntityType.Venue contains one ProcessedVenue.
    /// </summary>
    public record ProcessedVenue
    {
        /// <summary>
        /// Gets the venue name.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the slug identifier used for correlation.
        /// Generated from the venue name (e.g., "comedy-store" from "Comedy Store").
        /// Used to match events to venues and to deduplicate venues.
        /// </summary>
        public string? Slug { get; init; }

        /// <summary>
        /// Gets the timestamp when this record was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; init; }
    }
}