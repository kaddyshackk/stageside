namespace StageSide.Pipeline.Models
{
    /// <summary>
    /// Represents a processed act from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Comedian model.
    /// Each SilverRecord with EntityType.Act contains one ProcessedAct.
    /// </summary>
    public record ProcessedAct
    {
        /// <summary>
        /// Gets the act name.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the slug identifier used for correlation.
        /// For Punchup, this is extracted from the URL (e.g., "joe-list" from punchup.live/joe-list/tickets).
        /// Used to match events to comedians and to deduplicate comedians.
        /// </summary>
        public string? Slug { get; init; }

        /// <summary>
        /// Gets the comedian bio.
        /// </summary>
        public string? Bio { get; init; }

        /// <summary>
        /// Gets the timestamp when this record was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; init; }
    }
}
